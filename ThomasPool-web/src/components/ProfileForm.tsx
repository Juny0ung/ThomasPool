import { useEffect, useState } from 'react'
import { getProfileForm, submitProfile } from '../api/profileApi'
import { QuestionType } from '../api/types'
import type { AnyQuestion, ProfileContentDto, ProfileFormResponse } from '../api/types'

interface FixedFields {
  name: string
  gender: '' | 'true' | 'false'
  phoneNumber: string
  birthYear: string
  region: string
}

type Answer = string | boolean | string[]

function buildInfo(questions: AnyQuestion[], answers: Record<number, Answer>): ProfileContentDto[] {
  return questions.map((q, i) => {
    const answer = answers[i]
    if (q.questionType === QuestionType.TrueFalse) {
      return { type: 'bool', value: answer === true }
    }
    if (q.questionType === QuestionType.MultipleChoice) {
      return { type: 'multiple', values: answer != null ? [answer as string] : [] }
    }
    if (q.questionType === QuestionType.MultipleSelection) {
      return { type: 'multiple', values: (answer as string[]) ?? [] }
    }
    return { type: 'string', value: (answer as string) ?? '' }
  })
}

function QuestionField({
  question,
  index,
  answer,
  onChange,
}: {
  question: AnyQuestion
  index: number
  answer: Answer | undefined
  onChange: (index: number, value: Answer) => void
}) {
  const { question: label, questionType } = question

  if (questionType === QuestionType.ShortAnswer) {
    return (
      <div>
        <label>{label}</label>
        <input
          type="text"
          value={(answer as string) ?? ''}
          onChange={(e) => onChange(index, e.target.value)}
          required
        />
      </div>
    )
  }

  if (questionType === QuestionType.Essay) {
    return (
      <div>
        <label>{label}</label>
        <textarea
          value={(answer as string) ?? ''}
          onChange={(e) => onChange(index, e.target.value)}
          required
        />
      </div>
    )
  }

  if (questionType === QuestionType.TrueFalse) {
    return (
      <div>
        <label>{label}</label>
        <label>
          <input
            type="radio"
            name={`q-${index}`}
            checked={answer === true}
            onChange={() => onChange(index, true)}
            required
          />
          예
        </label>
        <label>
          <input
            type="radio"
            name={`q-${index}`}
            checked={answer === false}
            onChange={() => onChange(index, false)}
          />
          아니오
        </label>
      </div>
    )
  }

  if (questionType === QuestionType.MultipleChoice && question.type === 'multiple') {
    return (
      <div>
        <label>{label}</label>
        {question.options.map((opt) => (
          <label key={opt}>
            <input
              type="radio"
              name={`q-${index}`}
              value={opt}
              checked={answer === opt}
              onChange={() => onChange(index, opt)}
              required
            />
            {opt}
          </label>
        ))}
      </div>
    )
  }

  if (questionType === QuestionType.MultipleSelection && question.type === 'multiple') {
    const selected = (answer as string[]) ?? []
    return (
      <div>
        <label>{label}</label>
        {question.options.map((opt) => (
          <label key={opt}>
            <input
              type="checkbox"
              value={opt}
              checked={selected.includes(opt)}
              onChange={(e) => {
                const next = e.target.checked
                  ? [...selected, opt]
                  : selected.filter((v) => v !== opt)
                onChange(index, next)
              }}
            />
            {opt}
          </label>
        ))}
      </div>
    )
  }

  return null
}

export default function ProfileForm() {
  const [form, setForm] = useState<ProfileFormResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  const [fixed, setFixed] = useState<FixedFields>({
    name: '',
    gender: '',
    phoneNumber: '',
    birthYear: '',
    region: '',
  })
  const [answers, setAnswers] = useState<Record<number, Answer>>({})

  useEffect(() => {
    getProfileForm()
      .then(setForm)
      .catch(() => {})
      .finally(() => setLoading(false))
  }, [])

  function setAnswer(index: number, value: Answer) {
    setAnswers((prev) => ({ ...prev, [index]: value }))
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setSubmitError(null)
    try {
      await submitProfile({
        name: fixed.name,
        gender: fixed.gender === 'true',
        phoneNumber: fixed.phoneNumber,
        birthYear: parseInt(fixed.birthYear),
        region: fixed.region,
        version: form?.version ?? 0,
        info: form ? buildInfo(form.questions, answers) : [],
      })
      setSuccess(true)
    } catch {
      setSubmitError('제출에 실패했습니다. 다시 시도해주세요.')
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) return <p>불러오는 중...</p>
  if (success) return <p>제출이 완료되었습니다. 감사합니다!</p>

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label>이름</label>
        <input
          type="text"
          value={fixed.name}
          onChange={(e) => setFixed((p) => ({ ...p, name: e.target.value }))}
          required
        />
      </div>

      <div>
        <label>성별</label>
        <label>
          <input
            type="radio"
            name="gender"
            value="true"
            checked={fixed.gender === 'true'}
            onChange={() => setFixed((p) => ({ ...p, gender: 'true' }))}
            required
          />
          남
        </label>
        <label>
          <input
            type="radio"
            name="gender"
            value="false"
            checked={fixed.gender === 'false'}
            onChange={() => setFixed((p) => ({ ...p, gender: 'false' }))}
          />
          녀
        </label>
      </div>

      <div>
        <label>전화번호</label>
        <input
          type="tel"
          value={fixed.phoneNumber}
          onChange={(e) => setFixed((p) => ({ ...p, phoneNumber: e.target.value }))}
          pattern="\d{11}"
          placeholder="01012345678"
          required
        />
      </div>

      <div>
        <label>출생연도</label>
        <select
          value={fixed.birthYear}
          onChange={(e) => setFixed((p) => ({ ...p, birthYear: e.target.value }))}
          required
        >
          <option value="">선택</option>
          {Array.from({ length: new Date().getFullYear() - 20 - 1980 + 1 }, (_, i) => 1980 + i).map(
            (year) => (
              <option key={year} value={year}>
                {year}
              </option>
            ),
          )}
        </select>
      </div>

      <div>
        <label>지역</label>
        <input
          type="text"
          value={fixed.region}
          onChange={(e) => setFixed((p) => ({ ...p, region: e.target.value }))}
          required
        />
      </div>

      {form?.questions.map((q, i) => (
        <QuestionField
          key={i}
          question={q}
          index={i}
          answer={answers[i]}
          onChange={setAnswer}
        />
      ))}

      {submitError && <p>{submitError}</p>}

      <button type="submit" disabled={submitting}>
        {submitting ? '제출 중...' : '제출'}
      </button>
    </form>
  )
}
