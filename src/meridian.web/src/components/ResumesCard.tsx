import { useCallback, useEffect, useRef, useState } from 'react'
import { api, ApiError } from '../lib/api'
import { Badge, Card, EmptyState, Loading } from './ui'

interface ResumeSummary {
  id: number
  fileName: string
  sourceFormat: string
  sizeInBytes: number
  isPrimary: boolean
  createdAt: string
  downloadUrl: string
}

function formatSize(bytes: number) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

/**
 * Resume upload, listing and deletion. Files are stored in Cloudflare R2 via
 * POST /api/resumes; this component never sees the storage provider, only a
 * pre-signed download link handed back by the API.
 */
export function ResumesCard() {
  const [resumes, setResumes] = useState<ResumeSummary[] | null>(null)
  const [uploading, setUploading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const inputRef = useRef<HTMLInputElement>(null)

  const load = useCallback(() => {
    api.get<ResumeSummary[]>('/api/resumes').then(setResumes).catch(() => setResumes([]))
  }, [])

  useEffect(() => {
    load()
  }, [load])

  async function onFileChosen(file: File | undefined) {
    if (!file) return
    setError(null)
    setUploading(true)
    try {
      const form = new FormData()
      form.append('file', file)
      await api.upload('/api/resumes', form)
      load()
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'Upload failed. Please try again.')
    } finally {
      setUploading(false)
      if (inputRef.current) inputRef.current.value = ''
    }
  }

  async function onDelete(id: number) {
    try {
      await api.delete(`/api/resumes/${id}`)
      load()
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'Could not delete that resume.')
    }
  }

  return (
    <Card
      title="My resumes"
      action={
        <label className="cursor-pointer text-sm text-accent hover:underline">
          {uploading ? 'Uploading…' : 'Upload CV'}
          <input
            ref={inputRef}
            type="file"
            accept=".pdf,.doc,.docx,.txt"
            className="sr-only"
            disabled={uploading}
            onChange={(e) => onFileChosen(e.target.files?.[0])}
          />
        </label>
      }
    >
      {error && <p className="mb-3 text-sm text-danger">{error}</p>}

      {resumes === null ? (
        <Loading />
      ) : resumes.length === 0 ? (
        <EmptyState
          title="No resumes uploaded yet"
          detail="PDF, DOC, DOCX or TXT, up to 5 MB. Stored securely in cloud storage, not on this server."
        />
      ) : (
        <ul className="divide-y divide-line">
          {resumes.map((r) => (
            <li key={r.id} className="flex items-center justify-between gap-3 py-2.5">
              <div className="min-w-0">
                <a
                  href={r.downloadUrl}
                  target="_blank"
                  rel="noreferrer"
                  className="truncate font-medium text-ink-900 hover:text-accent"
                >
                  {r.fileName}
                </a>
                <p className="mt-0.5 text-xs text-ink-500">
                  {r.sourceFormat.toUpperCase()} &middot; {formatSize(r.sizeInBytes)}
                </p>
              </div>
              <div className="flex shrink-0 items-center gap-2">
                {r.isPrimary && <Badge tone="accent">Primary</Badge>}
                <button
                  type="button"
                  onClick={() => onDelete(r.id)}
                  className="text-xs text-ink-400 hover:text-danger"
                  aria-label={`Delete ${r.fileName}`}
                >
                  Delete
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}
    </Card>
  )
}
