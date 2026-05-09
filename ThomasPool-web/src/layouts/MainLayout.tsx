import { Outlet } from 'react-router-dom'

export default function MainLayout() {
  return (
    <div>
      <header>
        <nav>ThomasPool</nav>
      </header>
      <main>
        <Outlet />
      </main>
    </div>
  )
}
