import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { login, register } from '../../api/adminApi'
import { useAuth } from '../../contexts/AuthContext'
import ProfileForm from '../../components/ProfileForm'

type Mode = 'login' | 'register'

const inputClass =
  'w-full rounded-lg border border-gray-200 bg-white px-4 py-2.5 text-sm text-gray-800 outline-none transition focus:border-indigo-400 focus:ring-2 focus:ring-indigo-100'

const labelClass = 'mb-2 block text-sm font-medium text-gray-700'

export default function LoginPage() {
  const { token, saveToken } = useAuth()
  const navigate = useNavigate()

  useEffect(() => {
    if (token) navigate('/admin', { replace: true })
  }, [token])

  const [open, setOpen] = useState(true)
  const [mode, setMode] = useState<Mode>('login')
  const [adminId, setAdminId] = useState('')
  const [password, setPassword] = useState('')
  const [passwordConfirm, setPasswordConfirm] = useState('')
  const [name, setName] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [registered, setRegistered] = useState(false)

  function reset() {
    setAdminId('')
    setPassword('')
    setPasswordConfirm('')
    setName('')
    setError(null)
    setRegistered(false)
  }

  function switchMode(next: Mode) {
    reset()
    setMode(next)
  }

  async function handleLogin(e: React.FormEvent<HTMLFormElement>) {
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

  async function handleRegister(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault()
    if (password !== passwordConfirm) {
      setError('비밀번호가 일치하지 않습니다.')
      return
    }
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

  return (
    <div>
      <div className="fixed top-0 left-0 right-0 z-50 flex justify-center">
        <div className="w-full max-w-sm rounded-b-2xl bg-white shadow-xl ring-1 ring-gray-100">
          <button
            onClick={() => setOpen((v) => !v)}
            className="flex w-full items-center justify-between px-6 py-3 text-sm font-medium text-gray-700 hover:bg-gray-50"
          >
            <span>관리자</span>
            <span>{open ? '▲' : '▼'}</span>
          </button>

          {open && (
            <div className="px-6 pb-6">
              {registered ? (
                <div className="text-center">
                  <p className="font-medium text-gray-800">가입 신청이 완료되었습니다.</p>
                  <p className="mt-1 text-sm text-gray-500">관리자 승인 후 로그인하실 수 있습니다.</p>
                  <button
                    onClick={() => switchMode('login')}
                    className="mt-4 text-sm font-medium text-indigo-600 hover:underline"
                  >
                    로그인으로 돌아가기
                  </button>
                </div>
              ) : (
                <>
                  <div className="mb-5 flex gap-1 rounded-lg bg-gray-100 p-1">
                    {(['login', 'register'] as Mode[]).map((m) => (
                      <button
                        key={m}
                        onClick={() => switchMode(m)}
                        className={`flex-1 rounded-md py-1.5 text-sm font-medium transition ${
                          mode === m ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-500 hover:text-gray-700'
                        }`}
                      >
                        {m === 'login' ? '로그인' : '회원가입'}
                      </button>
                    ))}
                  </div>

                  {mode === 'login' ? (
                    <form onSubmit={handleLogin} className="space-y-4">
                      <div>
                        <label className={labelClass}>아이디</label>
                        <input
                          type="text"
                          className={inputClass}
                          value={adminId}
                          onChange={(e) => setAdminId(e.target.value)}
                          required
                        />
                      </div>
                      <div>
                        <label className={labelClass}>비밀번호</label>
                        <input
                          type="password"
                          className={inputClass}
                          value={password}
                          onChange={(e) => setPassword(e.target.value)}
                          required
                        />
                      </div>
                      {error && <p className="text-sm text-red-500">{error}</p>}
                      <button
                        type="submit"
                        disabled={loading}
                        className="w-full rounded-xl bg-indigo-600 py-2.5 text-sm font-semibold text-white transition hover:bg-indigo-700 disabled:opacity-50"
                      >
                        {loading ? '로그인 중...' : '로그인'}
                      </button>
                    </form>
                  ) : (
                    <form onSubmit={handleRegister} className="space-y-4">
                      <div>
                        <label className={labelClass}>이름</label>
                        <input
                          type="text"
                          className={inputClass}
                          value={name}
                          onChange={(e) => setName(e.target.value)}
                          minLength={1}
                          maxLength={100}
                          required
                        />
                      </div>
                      <div>
                        <label className={labelClass}>아이디</label>
                        <input
                          type="text"
                          className={inputClass}
                          value={adminId}
                          onChange={(e) => setAdminId(e.target.value)}
                          minLength={2}
                          maxLength={30}
                          required
                        />
                      </div>
                      <div>
                        <label className={labelClass}>비밀번호</label>
                        <input
                          type="password"
                          className={inputClass}
                          value={password}
                          onChange={(e) => setPassword(e.target.value)}
                          required
                        />
                      </div>
                      <div>
                        <label className={labelClass}>비밀번호 확인</label>
                        <input
                          type="password"
                          className={`${inputClass} ${passwordConfirm && password !== passwordConfirm ? 'border-red-300 focus:border-red-400 focus:ring-red-100' : ''}`}
                          value={passwordConfirm}
                          onChange={(e) => setPasswordConfirm(e.target.value)}
                          required
                        />
                        {passwordConfirm && password !== passwordConfirm && (
                          <p className="mt-1 text-xs text-red-500">비밀번호가 일치하지 않습니다.</p>
                        )}
                      </div>
                      {error && <p className="text-sm text-red-500">{error}</p>}
                      <button
                        type="submit"
                        disabled={loading}
                        className="w-full rounded-xl bg-indigo-600 py-2.5 text-sm font-semibold text-white transition hover:bg-indigo-700 disabled:opacity-50"
                      >
                        {loading ? '처리 중...' : '가입 신청'}
                      </button>
                    </form>
                  )}
                </>
              )}
            </div>
          )}
        </div>
      </div>

      <ProfileForm />
    </div>
  )
}
