import { Navigate, Outlet } from 'react-router-dom'

function isAdmin(): boolean {
  // TODO: 실제 인증 로직으로 교체
  return false
}

export default function AdminGuard() {
  if (!isAdmin()) {
    return <Navigate to="/" replace />
  }
  return <Outlet />
}
