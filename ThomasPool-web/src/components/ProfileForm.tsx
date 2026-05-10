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

const inputClass =
  'w-full rounded-lg border border-gray-200 bg-white px-4 py-2.5 text-sm text-gray-800 outline-none transition focus:border-indigo-400 focus:ring-2 focus:ring-indigo-100'

const labelClass = 'mb-2 block text-base font-semibold text-gray-800'

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
        <label className={labelClass}>{label}</label>
        <input
          type="text"
          className={inputClass}
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
        <label className={labelClass}>{label}</label>
        <textarea
          className={`${inputClass} min-h-24 resize-y`}
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
        <label className={labelClass}>{label}</label>
        <div className="flex gap-4">
          {[{ label: '예', value: true }, { label: '아니오', value: false }].map((opt) => (
            <label key={opt.label} className="flex cursor-pointer items-center gap-2 text-sm text-gray-700">
              <input
                type="radio"
                name={`q-${index}`}
                className="accent-indigo-500"
                checked={answer === opt.value}
                onChange={() => onChange(index, opt.value)}
                required={opt.value === true}
              />
              {opt.label}
            </label>
          ))}
        </div>
      </div>
    )
  }

  if (questionType === QuestionType.MultipleChoice && question.type === 'multiple') {
    return (
      <div>
        <label className={labelClass}>{label}</label>
        <div className="flex flex-col gap-2">
          {question.options.map((opt) => (
            <label key={opt} className="flex cursor-pointer items-center gap-2 text-sm text-gray-700">
              <input
                type="radio"
                name={`q-${index}`}
                value={opt}
                className="accent-indigo-500"
                checked={answer === opt}
                onChange={() => onChange(index, opt)}
                required
              />
              {opt}
            </label>
          ))}
        </div>
      </div>
    )
  }

  if (questionType === QuestionType.MultipleSelection && question.type === 'multiple') {
    const selected = (answer as string[]) ?? []
    return (
      <div>
        <label className={labelClass}>{label}</label>
        <div className="flex flex-col gap-2">
          {question.options.map((opt) => (
            <label key={opt} className="flex cursor-pointer items-center gap-2 text-sm text-gray-700">
              <input
                type="checkbox"
                value={opt}
                className="accent-indigo-500"
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

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <p className="text-sm text-gray-400">불러오는 중...</p>
      </div>
    )
  }

  if (success) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="text-center">
          <p className="text-lg font-medium text-gray-800">제출이 완료되었습니다.</p>
          <p className="mt-1 text-sm text-gray-500">참여해 주셔서 감사합니다!</p>
        </div>
      </div>
    )
  }

  const birthYears = Array.from(
    { length: new Date().getFullYear() - 20 - 1980 + 1 },
    (_, i) => 1980 + i,
  )

  return (
    <div className="min-h-screen bg-gray-50 px-4 py-12">
      <form
        onSubmit={handleSubmit}
        className="mx-auto w-full max-w-xl space-y-5"
      >
        <div className="rounded-2xl bg-white p-6 shadow-sm ring-1 ring-gray-100">
          <p className="mb-6 text-xl font-semibold text-gray-900">프로필 등록</p>
          <hr className="mb-6 border-gray-100" />

          <div className="space-y-4">
            <div>
              <label className={labelClass}>이름</label>
              <input
                type="text"
                className={inputClass}
                value={fixed.name}
                onChange={(e) => setFixed((p) => ({ ...p, name: e.target.value }))}
                placeholder="홍길동"
                required
              />
            </div>

            <div>
              <label className={labelClass}>성별</label>
              <div className="flex gap-4">
                {[{ label: '남', value: 'true' }, { label: '녀', value: 'false' }].map((opt) => (
                  <label key={opt.value} className="flex cursor-pointer items-center gap-2 text-sm text-gray-700">
                    <input
                      type="radio"
                      name="gender"
                      value={opt.value}
                      className="accent-indigo-500"
                      checked={fixed.gender === opt.value}
                      onChange={() => setFixed((p) => ({ ...p, gender: opt.value as 'true' | 'false' }))}
                      required={opt.value === 'true'}
                    />
                    {opt.label}
                  </label>
                ))}
              </div>
            </div>

            <div>
              <label className={labelClass}>전화번호</label>
              <input
                type="tel"
                className={inputClass}
                value={fixed.phoneNumber}
                onChange={(e) => setFixed((p) => ({ ...p, phoneNumber: e.target.value }))}
                pattern="\d{11}"
                placeholder="01012345678"
                required
              />
            </div>

            <div>
              <label className={labelClass}>출생연도</label>
              <select
                className={inputClass}
                value={fixed.birthYear}
                onChange={(e) => setFixed((p) => ({ ...p, birthYear: e.target.value }))}
                required
              >
                <option value="">선택하세요</option>
                {birthYears.map((year) => (
                  <option key={year} value={year}>{year}</option>
                ))}
              </select>
            </div>

            <div>
              <label className={labelClass}>지역</label>
              <input
                type="text"
                className={inputClass}
                value={fixed.region}
                onChange={(e) => setFixed((p) => ({ ...p, region: e.target.value }))}
                placeholder="서울"
                required
              />
            </div>
          </div>
        </div>

        {form?.questions.map((q, i) => (
          <div key={i} className="rounded-2xl bg-white p-6 shadow-sm ring-1 ring-gray-100">
            <QuestionField question={q} index={i} answer={answers[i]} onChange={setAnswer} />
          </div>
        ))}

        {submitError && (
          <p className="text-center text-sm text-red-500">{submitError}</p>
        )}

        <button
          type="submit"
          disabled={submitting}
          className="w-full rounded-xl bg-indigo-600 py-3 text-sm font-semibold text-white transition hover:bg-indigo-700 disabled:opacity-50"
        >
          {submitting ? '제출 중...' : '제출'}
        </button>
      </form>
    </div>
  )
}
