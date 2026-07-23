/**
 * HTTPS reverse proxy for the Meridian API, running as a Cloudflare Pages
 * Function on the same origin as the client.
 *
 * Why this exists: the hosting plan behind the API cannot issue a TLS
 * certificate, so the API is only reachable over HTTP. The client is served over
 * HTTPS, and a browser blocks an HTTPS page from calling an HTTP endpoint as
 * mixed content before the request is even sent.
 *
 * This function terminates TLS at Cloudflare's edge and forwards to the origin
 * server-side, where that rule does not apply. Because it runs on the client's
 * own origin at /api/*, the browser never makes a cross-origin request at all,
 * so no CORS negotiation is involved and no second hostname is exposed.
 *
 * The edge-to-origin hop is unencrypted. That is a stated constraint of the free
 * hosting tier, not a finished production posture; the deployed instance carries
 * seeded demonstration data only.
 */

interface Env {
  API_ORIGIN?: string
}

const DEFAULT_ORIGIN = 'http://meridiantalent.runasp.net'

/** Hop-by-hop and Cloudflare-injected headers that must not reach the origin. */
const STRIPPED_HEADERS = [
  'host',
  'connection',
  'keep-alive',
  'transfer-encoding',
  'upgrade',
  'cf-connecting-ip',
  'cf-ipcountry',
  'cf-ray',
  'cf-visitor',
  'cf-worker',
  'x-forwarded-proto',
  'x-forwarded-host',
]

export const onRequest: PagesFunction<Env> = async (context) => {
  const { request, env } = context

  const incoming = new URL(request.url)
  const origin = new URL(env.API_ORIGIN ?? DEFAULT_ORIGIN)
  const target = new URL(incoming.pathname + incoming.search, origin)

  const headers = new Headers(request.headers)
  for (const name of STRIPPED_HEADERS) {
    headers.delete(name)
  }
  headers.set('Host', origin.host)
  headers.set('X-Forwarded-Proto', 'https')
  headers.set('X-Forwarded-Host', incoming.host)

  const method = request.method.toUpperCase()
  const hasBody = method !== 'GET' && method !== 'HEAD'

  // IIS answers a body-less POST that carries no Content-Length with 411, where
  // Kestrel accepts it. The difference only appears once deployed, so it is
  // normalised here rather than left to surface as a confusing production-only
  // failure.
  if (hasBody && !headers.has('content-length') && !headers.has('transfer-encoding')) {
    headers.set('Content-Length', '0')
  }

  let response: Response
  try {
    response = await fetch(target.toString(), {
      method: request.method,
      headers,
      body: hasBody ? request.body : undefined,
      redirect: 'manual',
    })
  } catch (error) {
    return Response.json(
      {
        title: 'The API could not be reached',
        detail: error instanceof Error ? error.message : String(error),
        status: 502,
      },
      { status: 502 },
    )
  }

  const outgoing = new Headers(response.headers)

  // A redirect from the origin points at http://. Rewrite it onto this origin so
  // the browser is never sent to an insecure URL.
  const location = outgoing.get('location')
  if (location) {
    try {
      const redirected = new URL(location, origin)
      if (redirected.host === origin.host) {
        redirected.protocol = 'https:'
        redirected.host = incoming.host
        outgoing.set('location', redirected.toString())
      }
    } catch {
      // A malformed Location header is passed through exactly as sent.
    }
  }

  return new Response(response.body, {
    status: response.status,
    statusText: response.statusText,
    headers: outgoing,
  })
}
