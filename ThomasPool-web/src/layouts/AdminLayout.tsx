import { Outlet } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

export default function AdminLayout() {
  const { clearToken } = useAuth()

  return (
    <div>
      <aside>
        <nav>Admin</nav>
        <button onClick={clearToken}>로그아웃</button>
      </aside>
      <main>
        <Outlet />
      </main>
    </div>
  )
}
