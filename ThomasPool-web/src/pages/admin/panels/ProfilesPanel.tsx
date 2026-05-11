import { useState } from 'react'
import { getPhotoUrl, getProfiles } from '../../../api/profileApi'
import type { ProfileResponse } from '../../../api/types'
import { useAuth } from '../../../contexts/AuthContext'

const PAGE_SIZE = 20

function formatContent(content: ProfileResponse['info'][number]): string {
  if (content.type === 'bool') return content.value ? '예' : '아니오'
  if (content.type === 'multiple') return content.values.join(', ')
  return content.value
}

export default function ProfilesPanel() {
  const { token } = useAuth()
  const [nameInput, setNameInput] = useState('')
  const [profiles, setProfiles] = useState<ProfileResponse[]>([])
  const [skip, setSkip] = useState(0)
  const [selected, setSelected] = useState<ProfileResponse | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [searched, setSearched] = useState(false)

  async function fetchProfiles(name: string, offset: number) {
    setLoading(true)
    setError(null)
    try {
      const data = await getProfiles(token!, name, offset, PAGE_SIZE)
      setProfiles(data)
      setSkip(offset)
      setSelected(null)
      setSearched(true)
    } catch (e) {
      setError(e instanceof Error ? e.message : '오류가 발생했습니다.')
    } finally {
      setLoading(false)
    }
  }

  function handleSearch(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault()
    fetchProfiles(nameInput, 0)
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
            {[
              { label: '전화번호', value: selected.phoneNumber },
            ].map(({ label, value }) => (
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
      <form onSubmit={handleSearch} className="mb-4 flex gap-2">
        <input
          type="text"
          placeholder="이름으로 검색"
          value={nameInput}
          onChange={(e) => setNameInput(e.target.value)}
          className="w-full rounded-lg border border-gray-200 px-4 py-2.5 text-sm text-gray-800 outline-none transition focus:border-indigo-400 focus:ring-2 focus:ring-indigo-100"
        />
        <button
          type="submit"
          disabled={loading}
          className="rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-medium text-white hover:bg-indigo-700 transition disabled:opacity-50"
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
                onClick={() => fetchProfiles(nameInput, skip - PAGE_SIZE)}
                disabled={skip === 0 || loading}
                className="rounded-lg border border-gray-200 px-4 py-2 text-sm text-gray-600 hover:bg-gray-50 transition disabled:opacity-40"
              >
                이전
              </button>
              <button
                onClick={() => fetchProfiles(nameInput, skip + PAGE_SIZE)}
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
