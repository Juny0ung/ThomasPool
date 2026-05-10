import { lazy, Suspense } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import AdminLayout from '../layouts/AdminLayout'
import MainLayout from '../layouts/MainLayout'
import LoginPage from '../pages/admin/LoginPage'
import AdminGuard from './AdminGuard'

const DashboardPage = lazy(() => import('../pages/admin/DashboardPage'))

export default function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<MainLayout />} />
      <Route path="/admin/login" element={<LoginPage />} />
      <Route element={<AdminGuard />}>
        <Route element={<AdminLayout />}>
          <Route
            path="/admin"
            element={
              <Suspense fallback={null}>
                <DashboardPage />
              </Suspense>
            }
          />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
