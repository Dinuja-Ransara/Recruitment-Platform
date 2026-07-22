/**
 * Thin HTTP client for the Meridian API.
 *
 * Every request goes through here so that token attachment, JSON handling and
 * error shaping exist in exactly one place. Components never call fetch directly.
 */

const TOKEN_KEY = 'meridian.accessToken'

/**
 * Where the API lives.
 *
 * Empty in development, because the Vite dev server proxies /api to the backend
 * so the browser sees one origin. A deployed build sets VITE_API_BASE_URL to the
 * public API origin, and CORS on the API allows it explicitly.
 */
const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '')

export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

export const tokenStore = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (token: string) => localStorage.setItem(TOKEN_KEY, token),
  clear: () => localStorage.removeItem(TOKEN_KEY),
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = tokenStore.get()

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...init.headers,
    },
  })

  if (response.status === 204) {
    return undefined as T
  }

  const text = await response.text()
  const body = text ? JSON.parse(text) : null

  if (!response.ok) {
    // The API returns RFC 7807 problem details, so prefer its message over a
    // generic status line whenever one is present.
    const detail =
      body?.detail ?? body?.title ?? `Request failed with status ${response.status}`
    throw new ApiError(response.status, detail)
  }

  return body as T
}

export const api = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, payload: unknown) =>
    request<T>(path, { method: 'POST', body: JSON.stringify(payload) }),
  put: <T>(path: string, payload: unknown) =>
    request<T>(path, { method: 'PUT', body: JSON.stringify(payload) }),
  delete: <T>(path: string) => request<T>(path, { method: 'DELETE' }),
}

export interface UserSummary {
  id: number
  email: string
  fullName: string
  roles: string[]
  organizationName: string | null
}

export interface LoginResponse {
  accessToken: string
  expiresAtUtc: string
  user: UserSummary
}
