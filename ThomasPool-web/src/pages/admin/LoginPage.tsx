import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { login, register } from '../../api/adminApi'
import { useAuth } from '../../contexts/AuthContext'

type Mode = 'login' | 'register'

export default function LoginPage() {
  const { token, saveToken } = useAuth()
  const navigate = useNavigate()

  useEffect(() => {
    if (token) navigate('/admin', { replace: true })
  }, [token])
  const [mode, setMode] = useState<Mode>('login')

  const [adminId, setAdminId] = useState('')
  const [password, setPassword] = useState('')
  const [name, setName] = useState('')

  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [registered, setRegistered] = useState(false)

  function reset() {
    setAdminId('')
    setPassword('')
    setName('')
    setError(null)
    setRegistered(false)
  }

  function switchMode(next: Mode) {
    reset()
    setMode(next)
  }

  async function handleLogin(e: React.FormEvent) {
    e.preventDefault()
    setLoading(true)
    setError(null)
    try {
      const token = await login(adminId, password)
      saveToken(token)
    } catch (err) {
      setError(err instanceof Error ? err.message : '로그인에 실패했습니다.')
    } finally {
      setLoading(false)
    }
  }

  async function handleRegister(e: React.FormEvent) {
    e.preventDefault()
    setLoading(true)
    setError(null)
    try {
      await register(name, adminId, password)
      setRegistered(true)
    } catch (err) {
      setError(err instanceof Error ? err.message : '가입 신청에 실패했습니다.')
    } finally {
      setLoading(false)
    }
  }

  if (registered) {
    return (
      <div>
        <p>가입 신청이 완료되었습니다.</p>
        <p>관리자 승인 후 로그인하실 수 있습니다.</p>
        <button onClick={() => switchMode('login')}>로그인으로 돌아가기</button>
      </div>
    )
  }

  return (
    <div>
      <div>
        <button onClick={() => switchMode('login')} disabled={mode === 'login'}>
          로그인
        </button>
        <button onClick={() => switchMode('register')} disabled={mode === 'register'}>
          회원가입
        </button>
      </div>

      {mode === 'login' ? (
        <form onSubmit={handleLogin}>
          <div>
            <label>아이디</label>
            <input
              type="text"
              value={adminId}
              onChange={(e) => setAdminId(e.target.value)}
              required
            />
          </div>
          <div>
            <label>비밀번호</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>
          {error && <p>{error}</p>}
          <button type="submit" disabled={loading}>
            {loading ? '로그인 중...' : '로그인'}
          </button>
        </form>
      ) : (
        <form onSubmit={handleRegister}>
          <div>
            <label>이름</label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              minLength={1}
              maxLength={100}
              required
            />
          </div>
          <div>
            <label>아이디</label>
            <input
              type="text"
              value={adminId}
              onChange={(e) => setAdminId(e.target.value)}
              minLength={2}
              maxLength={30}
              required
            />
          </div>
          <div>
            <label>비밀번호</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>
          {error && <p>{error}</p>}
          <button type="submit" disabled={loading}>
            {loading ? '처리 중...' : '가입 신청'}
          </button>
        </form>
      )}
    </div>
  )
}
