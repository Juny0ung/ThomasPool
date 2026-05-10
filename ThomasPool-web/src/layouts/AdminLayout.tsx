import { Outlet } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

export default function AdminLayout() {
  const { clearToken } = useAuth()

  return (
    <div>
      <header className="fixed top-0 left-0 right-0 z-50 flex items-center justify-between bg-white px-6 py-3 shadow-sm ring-1 ring-gray-100">
        <span className="text-sm font-semibold text-gray-900">Admin</span>
        <button
          onClick={clearToken}
          className="text-sm font-medium text-gray-500 hover:text-gray-800 transition"
        >
          로그아웃
        </button>
      </header>
      <main className="pt-12">
        <Outlet />
      </main>
    </div>
  )
}
