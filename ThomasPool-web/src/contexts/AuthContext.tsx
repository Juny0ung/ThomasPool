import { createContext, useContext, useState } from 'react'

const TOKEN_KEY = 'admin_token'

function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return payload.exp * 1000 < Date.now()
  } catch {
    return true
  }
}

interface AuthContextValue {
  token: string | null
  saveToken: (token: string) => void
  clearToken: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(() => {
    const stored = localStorage.getItem(TOKEN_KEY)
    if (stored && isTokenExpired(stored)) {
      localStorage.removeItem(TOKEN_KEY)
      return null
    }
    return stored
  })

  function saveToken(t: string) {
    localStorage.setItem(TOKEN_KEY, t)
    setToken(t)
  }

  function clearToken() {
    localStorage.removeItem(TOKEN_KEY)
    setToken(null)
  }

  return (
    <AuthContext.Provider value={{ token, saveToken, clearToken }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
