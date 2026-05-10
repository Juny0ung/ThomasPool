import { useState } from 'react'
import { getProfiles } from '../../../api/profileApi'
import type { ProfileDto } from '../../../api/types'
import { useAuth } from '../../../contexts/AuthContext'
import { QuestionType } from '../../../api/types'

const PAGE_SIZE = 20

const QUESTION_TYPE_LABELS: Record<number, string> = {
  [QuestionType.ShortAnswer]: '단답형',
  [QuestionType.Essay]: '서술형',
  [QuestionType.MultipleChoice]: '객관식 (단일 선택)',
  [QuestionType.MultipleSelection]: '객관식 (복수 선택)',
  [QuestionType.TrueFalse]: '예/아니오',
}

function formatContent(content: ProfileDto['info'][number]): string {
  if (content.type === 'bool') return content.value ? '예' : '아니오'
  if (content.type === 'multiple') return content.values.join(', ')
  return content.value
}

export default function ProfilesPanel() {
  const { token } = useAuth()
  const [nameInput, setNameInput] = useState('')
  const [profiles, setProfiles] = useState<ProfileDto[]>([])
  const [skip, setSkip] = useState(0)
  const [selected, setSelected] = useState<ProfileDto | null>(null)
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

  function handleSearch(e: React.FormEvent) {
    e.preventDefault()
    fetchProfiles(nameInput, 0)
  }

  if (selected) {
    return (
      <div>
        <button onClick={() => setSelected(null)}>← 목록으로</button>

        <div>
          <div><span>이름</span><span>{selected.name}</span></div>
          <div><span>성별</span><span>{selected.gender ? '남' : '녀'}</span></div>
          <div><span>전화번호</span><span>{selected.phoneNumber}</span></div>
          <div><span>출생연도</span><span>{selected.birthYear}</span></div>
          <div><span>지역</span><span>{selected.region}</span></div>
        </div>

        {selected.info.length > 0 && (
          <div>
            {selected.info.map((content, i) => (
              <div key={i}>
                <span>항목 {i + 1}</span>
                <span>{formatContent(content)}</span>
              </div>
            ))}
          </div>
        )}
      </div>
    )
  }

  return (
    <div>
      <form onSubmit={handleSearch}>
        <input
          type="text"
          placeholder="이름으로 검색"
          value={nameInput}
          onChange={(e) => setNameInput(e.target.value)}
        />
        <button type="submit" disabled={loading}>검색</button>
      </form>

      {error && <p>{error}</p>}

      {loading ? (
        <p>불러오는 중...</p>
      ) : searched && profiles.length === 0 ? (
        <p>검색 결과가 없습니다.</p>
      ) : (
        <>
          {profiles.map((profile, i) => (
            <div key={i}>
              <button type="button" onClick={() => setSelected(profile)}>
                {profile.name} · {profile.gender ? '남' : '녀'} · {profile.birthYear}년 · {profile.region}
              </button>
            </div>
          ))}

          {searched && (
            <div>
              <button onClick={() => fetchProfiles(nameInput, skip - PAGE_SIZE)} disabled={skip === 0 || loading}>
                이전
              </button>
              <button onClick={() => fetchProfiles(nameInput, skip + PAGE_SIZE)} disabled={profiles.length < PAGE_SIZE || loading}>
                다음
              </button>
            </div>
          )}
        </>
      )}
    </div>
  )
}
