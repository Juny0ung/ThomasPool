import { Outlet } from 'react-router-dom'

export default function AdminLayout() {
  return (
    <div>
      <aside>
        <nav>Admin</nav>
      </aside>
      <main>
        <Outlet />
      </main>
    </div>
  )
}
