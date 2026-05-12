import { useEffect, useState } from 'react'
import { getPhotoUrl, getProfiles } from '../../../api/profileApi'
import type { ProfileFilter } from '../../../api/profileApi'
import type { ProfileResponse } from '../../../api/types'
import { UnauthorizedError } from '../../../api/errors'
import { useAuth } from '../../../contexts/AuthContext'

const PAGE_SIZE = 20

function formatContent(content: ProfileResponse['info'][number]): string {
  if (content.type === 'bool') return content.value ? '예' : '아니오'
  if (content.type === 'multiple') return content.values.join(', ')
  return content.value
}

const MIN_YEAR = 1980
const MAX_YEAR = new Date().getFullYear() - 20

function DualRangeSlider({
  min, max, valueMin, valueMax, onChange,
}: {
  min: number; max: number; valueMin: number; valueMax: number
  onChange: (min: number, max: number) => void
}) {
  const pct = (v: number) => ((v - min) / (max - min)) * 100
  return (
    <div className="px-1">
      <div className="relative h-5 flex items-center">
        <div className="absolute w-full h-1.5 rounded-full bg-gray-200" />
        <div
          className="absolute h-1.5 rounded-full bg-indigo-500"
          style={{ left: `${pct(valueMin)}%`, right: `${100 - pct(valueMax)}%` }}
        />
        <input
          type="range"
          min={min} max={max}
          value={valueMin}
          onChange={(e) => {
            const v = Number(e.target.value)
            onChange(Math.min(v, valueMax), valueMax)
          }}
          className="absolute w-full appearance-none bg-transparent [&::-webkit-slider-thumb]:appearance-none [&::-webkit-slider-thumb]:h-4 [&::-webkit-slider-thumb]:w-4 [&::-webkit-slider-thumb]:rounded-full [&::-webkit-slider-thumb]:bg-indigo-600 [&::-webkit-slider-thumb]:cursor-pointer [&::-webkit-slider-thumb]:border-2 [&::-webkit-slider-thumb]:border-white [&::-webkit-slider-thumb]:shadow pointer-events-none [&::-webkit-slider-thumb]:pointer-events-auto"
        />
        <input
          type="range"
          min={min} max={max}
          value={valueMax}
          onChange={(e) => {
            const v = Number(e.target.value)
            onChange(valueMin, Math.max(v, valueMin))
          }}
          className="absolute w-full appearance-none bg-transparent [&::-webkit-slider-thumb]:appearance-none [&::-webkit-slider-thumb]:h-4 [&::-webkit-slider-thumb]:w-4 [&::-webkit-slider-thumb]:rounded-full [&::-webkit-slider-thumb]:bg-indigo-600 [&::-webkit-slider-thumb]:cursor-pointer [&::-webkit-slider-thumb]:border-2 [&::-webkit-slider-thumb]:border-white [&::-webkit-slider-thumb]:shadow pointer-events-none [&::-webkit-slider-thumb]:pointer-events-auto"
        />
      </div>
      <div className="mt-1 flex justify-between text-xs text-gray-500">
        <span>{valueMin}</span>
        <span>{valueMax}</span>
      </div>
    </div>
  )
}

export default function ProfilesPanel() {
  const { token, clearToken } = useAuth()
  const [nameInput, setNameInput] = useState('')
  const [regionInput, setRegionInput] = useState('')
  const [regionTags, setRegionTags] = useState<string[]>([])
  const [genderFilter, setGenderFilter] = useState<boolean[]>([])
  const [birthYearMin, setBirthYearMin] = useState<number>(MIN_YEAR)
  const [birthYearMax, setBirthYearMax] = useState<number>(MAX_YEAR)
  const [profiles, setProfiles] = useState<ProfileResponse[]>([])
  const [skip, setSkip] = useState(0)
  const [selected, setSelected] = useState<ProfileResponse | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [searched, setSearched] = useState(false)
  const [currentFilter, setCurrentFilter] = useState<ProfileFilter>({})

  useEffect(() => {
    fetchProfiles(0, {})
  }, [])

  function buildFilter(): ProfileFilter {
    const filter: ProfileFilter = {}
    if (nameInput.trim()) filter.name = [nameInput.trim()]
    if (regionTags.length > 0) filter.region = regionTags
    if (genderFilter.length > 0) filter.gender = genderFilter
    if (birthYearMin !== MIN_YEAR || birthYearMax !== MAX_YEAR) {
      filter.birthYear = Array.from({ length: birthYearMax - birthYearMin + 1 }, (_, i) => birthYearMin + i)
    }
    return filter
  }

  async function fetchProfiles(offset: number, filter: ProfileFilter) {
    setLoading(true)
    setError(null)
    try {
      const data = await getProfiles(token!, offset, PAGE_SIZE, filter)
      setProfiles(data)
      setSkip(offset)
      setSelected(null)
      setSearched(true)
    } catch (e) {
      if (e instanceof UnauthorizedError) { clearToken(); return }
      setError(e instanceof Error ? e.message : '오류가 발생했습니다.')
    } finally {
      setLoading(false)
    }
  }

  function handleSearch(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault()
    const filter = buildFilter()
    setCurrentFilter(filter)
    fetchProfiles(0, filter)
  }

  function handleRegionKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    if (e.key === 'Enter') {
      e.preventDefault()
      const val = regionInput.trim()
      if (val && !regionTags.includes(val)) {
        setRegionTags((prev) => [...prev, val])
      }
      setRegionInput('')
    }
  }

  function toggleGender(value: boolean) {
    setGenderFilter((prev) =>
      prev.includes(value) ? prev.filter((v) => v !== value) : [...prev, value]
    )
  }

  if (selected) {
    return (
      <div className="mx-auto max-w-2xl">
        <button
          onClick={() => setSelected(null)}
          className="mb-4 text-sm font-medium text-indigo-600 hover:underline"
        >
          ← 목록으로
        </button>

        <div className="rounded-2xl bg-white p-6 shadow-sm ring-1 ring-gray-100">
          <div className="flex items-center gap-5 mb-6">
            <img
              src={getPhotoUrl(selected.photoId)}
              alt={selected.name}
              className="h-20 w-20 rounded-full object-cover ring-2 ring-gray-100"
            />
            <div>
              <p className="text-lg font-semibold text-gray-900">{selected.name}</p>
              <p className="text-sm text-gray-500">{selected.gender ? '남' : '녀'} · {selected.birthYear}년 · {selected.region}</p>
            </div>
          </div>

          <hr className="mb-4 border-gray-100" />

          <div className="space-y-3">
            {[{ label: '전화번호', value: selected.phoneNumber }].map(({ label, value }) => (
              <div key={label} className="flex justify-between text-sm">
                <span className="font-medium text-gray-500">{label}</span>
                <span className="text-gray-800">{value}</span>
              </div>
            ))}

            {selected.info.map((content, i) => (
              <div key={i} className="flex justify-between text-sm">
                <span className="font-medium text-gray-500">항목 {i + 1}</span>
                <span className="text-gray-800">{formatContent(content)}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-2xl">
      <form onSubmit={handleSearch} className="mb-4 space-y-3 rounded-2xl bg-white p-4 shadow-sm ring-1 ring-gray-100">
        <input
          type="text"
          placeholder="이름"
          value={nameInput}
          onChange={(e) => setNameInput(e.target.value)}
          className="w-full rounded-lg border border-gray-200 px-4 py-2.5 text-sm text-gray-800 outline-none transition focus:border-indigo-400 focus:ring-2 focus:ring-indigo-100"
        />

        <div>
          <input
            type="text"
            placeholder="지역 입력 후 Enter"
            value={regionInput}
            onChange={(e) => setRegionInput(e.target.value)}
            onKeyDown={handleRegionKeyDown}
            className="w-full rounded-lg border border-gray-200 px-4 py-2.5 text-sm text-gray-800 outline-none transition focus:border-indigo-400 focus:ring-2 focus:ring-indigo-100"
          />
          {regionTags.length > 0 && (
            <div className="mt-2 flex flex-wrap gap-1.5">
              {regionTags.map((tag) => (
                <span key={tag} className="flex items-center gap-1 rounded-full bg-indigo-50 px-3 py-1 text-xs font-medium text-indigo-700 ring-1 ring-indigo-200">
                  {tag}
                  <button
                    type="button"
                    onClick={() => setRegionTags((prev) => prev.filter((t) => t !== tag))}
                    className="text-indigo-400 hover:text-indigo-600"
                  >
                    ×
                  </button>
                </span>
              ))}
            </div>
          )}
        </div>

        <div className="flex flex-wrap items-center gap-4 text-sm text-gray-700">
          <span className="font-medium text-gray-500">성별</span>
          {[{ label: '남', value: true }, { label: '녀', value: false }].map((opt) => (
            <label key={opt.label} className="flex cursor-pointer items-center gap-1.5">
              <input
                type="checkbox"
                className="accent-indigo-500"
                checked={genderFilter.includes(opt.value)}
                onChange={() => toggleGender(opt.value)}
              />
              {opt.label}
            </label>
          ))}
        </div>

        <div>
          <p className="mb-2 text-sm font-medium text-gray-500">출생연도</p>
          <DualRangeSlider
            min={MIN_YEAR}
            max={MAX_YEAR}
            valueMin={birthYearMin}
            valueMax={birthYearMax}
            onChange={(mn, mx) => { setBirthYearMin(mn); setBirthYearMax(mx) }}
          />
        </div>

        <button
          type="submit"
          disabled={loading}
          className="w-full rounded-lg bg-indigo-600 py-2.5 text-sm font-medium text-white hover:bg-indigo-700 transition disabled:opacity-50"
        >
          검색
        </button>
      </form>

      {error && <p className="text-sm text-red-500">{error}</p>}

      {loading ? (
        <p className="text-center text-sm text-gray-400">불러오는 중...</p>
      ) : searched && profiles.length === 0 ? (
        <p className="text-center text-sm text-gray-400">검색 결과가 없습니다.</p>
      ) : (
        <>
          <div className="space-y-2">
            {profiles.map((profile, i) => (
              <button
                key={i}
                type="button"
                onClick={() => setSelected(profile)}
                className="flex w-full items-center gap-4 rounded-xl bg-white p-4 shadow-sm ring-1 ring-gray-100 transition hover:ring-indigo-200 text-left"
              >
                <img
                  src={getPhotoUrl(profile.photoId)}
                  alt={profile.name}
                  className="h-10 w-10 rounded-full object-cover"
                />
                <div>
                  <p className="text-sm font-medium text-gray-900">{profile.name}</p>
                  <p className="text-xs text-gray-400">{profile.gender ? '남' : '녀'} · {profile.birthYear}년 · {profile.region}</p>
                </div>
              </button>
            ))}
          </div>

          {searched && (
            <div className="mt-4 flex justify-center gap-2">
              <button
                onClick={() => fetchProfiles(skip - PAGE_SIZE, currentFilter)}
                disabled={skip === 0 || loading}
                className="rounded-lg border border-gray-200 px-4 py-2 text-sm text-gray-600 hover:bg-gray-50 transition disabled:opacity-40"
              >
                이전
              </button>
              <button
                onClick={() => fetchProfiles(skip + PAGE_SIZE, currentFilter)}
                disabled={profiles.length < PAGE_SIZE || loading}
                className="rounded-lg border border-gray-200 px-4 py-2 text-sm text-gray-600 hover:bg-gray-50 transition disabled:opacity-40"
              >
                다음
              </button>
            </div>
          )}
        </>
      )}
    </div>
  )
}
