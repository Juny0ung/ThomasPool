import { useEffect, useState } from 'react'
import { getProfileForm, updateProfileForm } from '../../../api/profileApi'
import { QuestionType } from '../../../api/types'
import type { AnyQuestion } from '../../../api/types'
import { UnauthorizedError } from '../../../api/errors'
import { useAuth } from '../../../contexts/AuthContext'

const QUESTION_TYPE_LABELS: Record<number, string> = {
  [QuestionType.ShortAnswer]: '단답형',
  [QuestionType.Essay]: '서술형',
  [QuestionType.MultipleChoice]: '객관식 (단일 선택)',
  [QuestionType.MultipleSelection]: '객관식 (복수 선택)',
  [QuestionType.TrueFalse]: '예/아니오',
}

const inputClass =
  'w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm text-gray-800 outline-none transition focus:border-indigo-400 focus:ring-2 focus:ring-indigo-100'

type EditableQuestion =
  | { type: 'question'; question: string; questionType: 0 | 1 | 4 }
  | { type: 'multiple'; question: string; questionType: 2 | 3; options: string[] }

function toEditable(q: AnyQuestion): EditableQuestion {
  if (q.type === 'multiple') {
    return { type: 'multiple', question: q.question, questionType: q.questionType as 2 | 3, options: [...q.options] }
  }
  return { type: 'question', question: q.question, questionType: q.questionType as 0 | 1 | 4 }
}

function toDto(q: EditableQuestion): AnyQuestion {
  return q as AnyQuestion
}

export default function ProfileFormPanel() {
  const { token, clearToken } = useAuth()
  const [questions, setQuestions] = useState<EditableQuestion[]>([])
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  useEffect(() => {
    getProfileForm()
      .then((f) => setQuestions(f.questions.map(toEditable)))
      .catch(() => setQuestions([]))
      .finally(() => setLoading(false))
  }, [])

  function updateQuestion(index: number, updated: EditableQuestion) {
    setQuestions((prev) => prev.map((q, i) => (i === index ? updated : q)))
  }

  function removeQuestion(index: number) {
    setQuestions((prev) => prev.filter((_, i) => i !== index))
  }

  function addQuestion(questionType: number) {
    const isMultiple = questionType === QuestionType.MultipleChoice || questionType === QuestionType.MultipleSelection
    const newQ: EditableQuestion = isMultiple
      ? { type: 'multiple', question: '', questionType: questionType as 2 | 3, options: ['', ''] }
      : { type: 'question', question: '', questionType: questionType as 0 | 1 | 4 }
    setQuestions((prev) => [...prev, newQ])
  }

  function updateOption(qIndex: number, oIndex: number, value: string) {
    setQuestions((prev) =>
      prev.map((q, i) => {
        if (i !== qIndex || q.type !== 'multiple') return q
        return { ...q, options: q.options.map((o, j) => (j === oIndex ? value : o)) }
      }),
    )
  }

  function addOption(qIndex: number) {
    setQuestions((prev) =>
      prev.map((q, i) => {
        if (i !== qIndex || q.type !== 'multiple') return q
        return { ...q, options: [...q.options, ''] }
      }),
    )
  }

  function removeOption(qIndex: number, oIndex: number) {
    setQuestions((prev) =>
      prev.map((q, i) => {
        if (i !== qIndex || q.type !== 'multiple') return q
        return { ...q, options: q.options.filter((_, j) => j !== oIndex) }
      }),
    )
  }

  async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault()
    setSubmitting(true)
    setError(null)
    setSuccess(false)
    try {
      await updateProfileForm(token!, questions.map(toDto))
      setSuccess(true)
    } catch (err) {
      if (err instanceof UnauthorizedError) { clearToken(); return }
      setError(err instanceof Error ? err.message : '오류가 발생했습니다.')
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) return <p className="text-sm text-gray-400">불러오는 중...</p>

  return (
    <form onSubmit={handleSubmit} className="mx-auto max-w-2xl space-y-4">
      {questions.length === 0 && (
        <p className="text-center text-sm text-gray-400">질문이 없습니다. 아래에서 추가해주세요.</p>
      )}

      {questions.map((q, qIndex) => (
        <div key={qIndex} className="rounded-2xl bg-white p-5 shadow-sm ring-1 ring-gray-100">
          <div className="mb-3 flex items-center justify-between">
            <span className="rounded-full bg-indigo-50 px-3 py-0.5 text-xs font-medium text-indigo-600">
              {QUESTION_TYPE_LABELS[q.questionType]}
            </span>
            <button
              type="button"
              onClick={() => removeQuestion(qIndex)}
              className="text-xs text-gray-400 hover:text-red-500 transition"
            >
              삭제
            </button>
          </div>

          <input
            type="text"
            className={inputClass}
            placeholder="질문을 입력하세요"
            value={q.question}
            onChange={(e) => updateQuestion(qIndex, { ...q, question: e.target.value })}
            required
          />

          {q.type === 'multiple' && (
            <div className="mt-3 space-y-2">
              {q.options.map((opt, oIndex) => (
                <div key={oIndex} className="flex items-center gap-2">
                  <span className="text-xs text-gray-400 w-4">{oIndex + 1}</span>
                  <input
                    type="text"
                    className={inputClass}
                    placeholder={`선택지 ${oIndex + 1}`}
                    value={opt}
                    onChange={(e) => updateOption(qIndex, oIndex, e.target.value)}
                    required
                  />
                  {q.options.length > 2 && (
                    <button
                      type="button"
                      onClick={() => removeOption(qIndex, oIndex)}
                      className="shrink-0 text-gray-400 hover:text-red-500 transition text-sm"
                    >
                      ✕
                    </button>
                  )}
                </div>
              ))}
              <button
                type="button"
                onClick={() => addOption(qIndex)}
                className="mt-1 text-xs font-medium text-indigo-500 hover:text-indigo-700 transition"
              >
                + 선택지 추가
              </button>
            </div>
          )}
        </div>
      ))}

      <div className="rounded-2xl bg-white p-5 shadow-sm ring-1 ring-gray-100">
        <p className="mb-3 text-xs font-medium text-gray-500">질문 추가</p>
        <div className="flex flex-wrap gap-2">
          {Object.entries(QUESTION_TYPE_LABELS).map(([type, label]) => (
            <button
              key={type}
              type="button"
              onClick={() => addQuestion(Number(type))}
              className="rounded-lg border border-gray-200 px-3 py-1.5 text-xs font-medium text-gray-600 hover:border-indigo-300 hover:text-indigo-600 transition"
            >
              + {label}
            </button>
          ))}
        </div>
      </div>

      {error && <p className="text-center text-sm text-red-500">{error}</p>}
      {success && <p className="text-center text-sm text-indigo-600">폼이 업데이트 되었습니다.</p>}

      <button
        type="submit"
        disabled={submitting || questions.length === 0}
        className="w-full rounded-xl bg-indigo-600 py-3 text-sm font-semibold text-white transition hover:bg-indigo-700 disabled:opacity-50"
      >
        {submitting ? '저장 중...' : '저장'}
      </button>
    </form>
  )
}
