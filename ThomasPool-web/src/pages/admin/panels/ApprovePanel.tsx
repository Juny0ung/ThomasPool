import { useEffect, useState } from 'react'
import { approveAdmins, getPendingList } from '../../../api/adminApi'
import type { AdminDto } from '../../../api/adminApi'
import { useAuth } from '../../../contexts/AuthContext'

const PAGE_SIZE = 20

export default function ApprovePanel() {
  const { token } = useAuth()
  const [list, setList] = useState<AdminDto[]>([])
  const [selected, setSelected] = useState<Set<string>>(new Set())
  const [skip, setSkip] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function fetchList(offset: number) {
    setLoading(true)
    setError(null)
    try {
      const data = await getPendingList(token!, offset, PAGE_SIZE)
      setList(data)
      setSkip(offset)
      setSelected(new Set())
    } catch (e) {
      setError(e instanceof Error ? e.message : '오류가 발생했습니다.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchList(0) }, [])

  function toggleSelect(id: string) {
    setSelected((prev) => {
      const next = new Set(prev)
      next.has(id) ? next.delete(id) : next.add(id)
      return next
    })
  }

  async function handleApprove() {
    if (selected.size === 0) return
    setLoading(true)
    setError(null)
    try {
      await approveAdmins(token!, [...selected])
      await fetchList(skip)
    } catch (e) {
      setError(e instanceof Error ? e.message : '오류가 발생했습니다.')
      setLoading(false)
    }
  }

  return (
    <div>
      {error && <p>{error}</p>}

      {loading ? (
        <p>불러오는 중...</p>
      ) : list.length === 0 ? (
        <p>승인 대기 중인 계정이 없습니다.</p>
      ) : (
        <>
          {list.map((admin) => (
            <div key={admin.id}>
              <label>
                <input
                  type="checkbox"
                  checked={selected.has(admin.id)}
                  onChange={() => toggleSelect(admin.id)}
                />
                {admin.name} ({admin.id})
              </label>
            </div>
          ))}

          <div>
            <button onClick={() => fetchList(skip - PAGE_SIZE)} disabled={skip === 0 || loading}>
              이전
            </button>
            <button onClick={() => fetchList(skip + PAGE_SIZE)} disabled={list.length < PAGE_SIZE || loading}>
              다음
            </button>
          </div>

          <button onClick={handleApprove} disabled={selected.size === 0 || loading}>
            선택 승인 ({selected.size})
          </button>
        </>
      )}
    </div>
  )
}
