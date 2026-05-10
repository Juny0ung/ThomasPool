import { useEffect, useState } from 'react'
import { getProfileForm, updateProfileForm } from '../../../api/profileApi'
import { QuestionType } from '../../../api/types'
import type { AnyQuestion } from '../../../api/types'
import { useAuth } from '../../../contexts/AuthContext'

const QUESTION_TYPE_LABELS: Record<number, string> = {
  [QuestionType.ShortAnswer]: '단답형',
  [QuestionType.Essay]: '서술형',
  [QuestionType.MultipleChoice]: '객관식 (단일 선택)',
  [QuestionType.MultipleSelection]: '객관식 (복수 선택)',
  [QuestionType.TrueFalse]: '예/아니오',
}

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
  const { token } = useAuth()
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
        const options = q.options.map((o, j) => (j === oIndex ? value : o))
        return { ...q, options }
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

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setError(null)
    setSuccess(false)
    try {
      await updateProfileForm(token!, questions.map(toDto))
      setSuccess(true)
    } catch (err) {
      setError(err instanceof Error ? err.message : '오류가 발생했습니다.')
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) return <p>불러오는 중...</p>

  return (
    <form onSubmit={handleSubmit}>
      {questions.map((q, qIndex) => (
        <div key={qIndex}>
          <div>
            <span>{QUESTION_TYPE_LABELS[q.questionType]}</span>
            <button type="button" onClick={() => removeQuestion(qIndex)}>삭제</button>
          </div>

          <input
            type="text"
            placeholder="질문을 입력하세요"
            value={q.question}
            onChange={(e) => updateQuestion(qIndex, { ...q, question: e.target.value })}
            required
          />

          {q.type === 'multiple' && (
            <div>
              {q.options.map((opt, oIndex) => (
                <div key={oIndex}>
                  <input
                    type="text"
                    placeholder={`선택지 ${oIndex + 1}`}
                    value={opt}
                    onChange={(e) => updateOption(qIndex, oIndex, e.target.value)}
                    required
                  />
                  {q.options.length > 2 && (
                    <button type="button" onClick={() => removeOption(qIndex, oIndex)}>-</button>
                  )}
                </div>
              ))}
              <button type="button" onClick={() => addOption(qIndex)}>선택지 추가</button>
            </div>
          )}
        </div>
      ))}

      <div>
        <span>질문 추가: </span>
        {Object.entries(QUESTION_TYPE_LABELS).map(([type, label]) => (
          <button key={type} type="button" onClick={() => addQuestion(Number(type))}>
            + {label}
          </button>
        ))}
      </div>

      {error && <p>{error}</p>}
      {success && <p>폼이 업데이트 되었습니다.</p>}

      <button type="submit" disabled={submitting || questions.length === 0}>
        {submitting ? '저장 중...' : '저장'}
      </button>
    </form>
  )
}
