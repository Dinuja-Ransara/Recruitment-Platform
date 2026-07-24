import { useCallback, useEffect, useRef, useState } from 'react'

/**
 * Floating candidate/recruiter support chatbot, backed by Cloudflare Workers
 * AI through functions/api/chat.ts. Mounted once in AppShell so it is
 * available on every signed-in screen. Ported from the same pattern already
 * proven on hasithabandara.com (Dilly), restyled onto Meridian's own tokens
 * instead of introducing a second design language.
 */

interface Message {
  role: 'user' | 'assistant'
  content: string
}

const GREETING =
  "Hi, I'm Meridian AI. Ask me how to search jobs, apply, rank applicants, or anything else about the platform."

const SUGGESTIONS = ['How does the scoring work?', 'How do I apply for a job?', 'How do I publish a posting?']

export function MeridianAssistant() {
  const [open, setOpen] = useState(false)
  const [messages, setMessages] = useState<Message[]>([{ role: 'assistant', content: GREETING }])
  const [input, setInput] = useState('')
  const [busy, setBusy] = useState(false)
  const scrollRef = useRef<HTMLDivElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    if (scrollRef.current) scrollRef.current.scrollTop = scrollRef.current.scrollHeight
  }, [messages, open])

  useEffect(() => {
    if (open) inputRef.current?.focus()
  }, [open])

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setOpen(false)
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [])

  const send = useCallback(
    async (text?: string) => {
      const content = (text ?? input).trim()
      if (!content || busy) return
      setInput('')

      const next = [...messages, { role: 'user' as const, content }]
      setMessages([...next, { role: 'assistant', content: '' }])
      setBusy(true)

      try {
        const res = await fetch('/api/chat', {
          method: 'POST',
          headers: { 'content-type': 'application/json' },
          body: JSON.stringify({ messages: next }),
        })

        if (!res.ok || !res.body) throw new Error('bad response')

        const reader = res.body.getReader()
        const decoder = new TextDecoder()
        let buffer = ''
        let acc = ''

        for (;;) {
          const { done, value } = await reader.read()
          if (done) break
          buffer += decoder.decode(value, { stream: true })
          const lines = buffer.split('\n')
          buffer = lines.pop() || ''
          for (const line of lines) {
            const t = line.trim()
            if (!t.startsWith('data:')) continue
            const data = t.slice(5).trim()
            if (data === '[DONE]' || data === '') continue
            try {
              const parsed = JSON.parse(data) as { response?: string }
              if (parsed.response) {
                acc += parsed.response
                setMessages((prev) => {
                  const copy = prev.slice()
                  copy[copy.length - 1] = { role: 'assistant', content: acc }
                  return copy
                })
              }
            } catch {
              // partial frame or keep-alive, ignored
            }
          }
        }

        if (!acc.trim()) {
          setMessages((prev) => {
            const copy = prev.slice()
            copy[copy.length - 1] = {
              role: 'assistant',
              content: "I couldn't reach the assistant just now. Please try again.",
            }
            return copy
          })
        }
      } catch {
        setMessages((prev) => {
          const copy = prev.slice()
          copy[copy.length - 1] = {
            role: 'assistant',
            content: 'Something went wrong on my end. Please try again in a moment.',
          }
          return copy
        })
      } finally {
        setBusy(false)
      }
    },
    [input, busy, messages],
  )

  return (
    <>
      <button
        type="button"
        aria-label={open ? 'Close Meridian AI' : 'Open Meridian AI'}
        aria-expanded={open}
        onClick={() => setOpen((v) => !v)}
        className="fixed right-5 bottom-5 z-50 flex h-13 items-center gap-2 rounded-full bg-accent px-4 text-sm font-medium text-white shadow-[0_8px_24px_rgba(255,80,0,.35)] transition-transform hover:scale-105 hover:bg-accent-hover"
      >
        {open ? (
          <svg width="16" height="16" viewBox="0 0 16 16" aria-hidden="true">
            <path d="M3 3l10 10M13 3L3 13" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
          </svg>
        ) : (
          <>
            <svg width="16" height="16" viewBox="0 0 16 16" aria-hidden="true">
              <path
                d="M8 1a5 5 0 0 1 5 5c0 1.3-.5 2.4-1.3 3.3L14 13l-3.4-1.6A5 5 0 1 1 8 1Z"
                fill="none"
                stroke="currentColor"
                strokeWidth="1.4"
              />
            </svg>
            Ask Meridian AI
          </>
        )}
      </button>

      {open && (
        <section
          role="dialog"
          aria-label="Meridian AI chat"
          className="fixed right-5 bottom-21 z-50 flex h-110 w-90 max-w-[calc(100vw-2.5rem)] flex-col overflow-hidden rounded-[6px] border border-line bg-surface shadow-[0_16px_48px_rgba(10,17,32,.18)]"
        >
          <header className="flex items-center gap-2 border-b border-line bg-surface-alt px-4 py-3">
            <span className="flex h-8 w-8 items-center justify-center rounded-full bg-accent-soft text-accent">
              <svg width="16" height="16" viewBox="0 0 16 16" aria-hidden="true">
                <path
                  d="M8 1a5 5 0 0 1 5 5c0 1.3-.5 2.4-1.3 3.3L14 13l-3.4-1.6A5 5 0 1 1 8 1Z"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="1.4"
                />
              </svg>
            </span>
            <span className="leading-tight">
              <strong className="block text-sm font-semibold text-ink-900">Meridian AI</strong>
              <em className="block text-xs text-ink-400 not-italic">AI, can make mistakes</em>
            </span>
          </header>

          <div ref={scrollRef} className="flex-1 space-y-2.5 overflow-y-auto p-3">
            {messages.map((m, i) => (
              <div
                key={i}
                className={
                  m.role === 'user'
                    ? 'ml-auto max-w-[85%] rounded-[6px] bg-accent px-3 py-2 text-sm text-white'
                    : 'mr-auto max-w-[85%] rounded-[6px] bg-surface-alt px-3 py-2 text-sm text-ink-900'
                }
              >
                {m.content || <TypingDots />}
              </div>
            ))}

            {messages.length <= 1 && (
              <div className="flex flex-wrap gap-1.5 pt-1">
                {SUGGESTIONS.map((s) => (
                  <button
                    key={s}
                    type="button"
                    onClick={() => send(s)}
                    disabled={busy}
                    className="rounded-full border border-line-strong px-2.5 py-1 text-xs text-ink-700 transition-colors hover:border-accent hover:text-accent disabled:opacity-50"
                  >
                    {s}
                  </button>
                ))}
              </div>
            )}
          </div>

          <form
            onSubmit={(e) => {
              e.preventDefault()
              send()
            }}
            className="flex items-center gap-2 border-t border-line p-2.5"
          >
            <input
              ref={inputRef}
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder="Ask a question..."
              aria-label="Message the assistant"
              maxLength={500}
              className="min-w-0 flex-1 rounded-full border border-line-strong bg-surface px-3.5 py-2 text-sm outline-none placeholder:text-ink-300 focus:border-accent focus:ring-4 focus:ring-accent-soft"
            />
            <button
              type="submit"
              aria-label="Send"
              disabled={busy || !input.trim()}
              className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-accent text-white transition-opacity disabled:opacity-40"
            >
              <svg width="14" height="14" viewBox="0 0 16 16" aria-hidden="true">
                <path
                  d="M2 8h11M8 3l5 5-5 5"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </svg>
            </button>
          </form>
        </section>
      )}
    </>
  )
}

function TypingDots() {
  return (
    <span className="inline-flex items-center gap-1" aria-label="Assistant is typing">
      {[0, 1, 2].map((i) => (
        <span
          key={i}
          className="h-1.5 w-1.5 animate-bounce rounded-full bg-ink-300"
          style={{ animationDelay: `${i * 120}ms` }}
        />
      ))}
    </span>
  )
}
