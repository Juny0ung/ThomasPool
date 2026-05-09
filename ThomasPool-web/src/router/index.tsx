import { lazy, Suspense } from 'react'
import { createBrowserRouter } from 'react-router-dom'
import MainLayout from '../layouts/MainLayout'
import AdminLayout from '../layouts/AdminLayout'
import AdminGuard from './AdminGuard'

const HomePage = lazy(() => import('../pages/HomePage'))
const DashboardPage = lazy(() => import('../pages/admin/DashboardPage'))

const router = createBrowserRouter([
  {
    element: <MainLayout />,
    children: [
      {
        path: '/',
        element: (
          <Suspense fallback={null}>
            <HomePage />
          </Suspense>
        ),
      },
    ],
  },
  {
    element: <AdminGuard />,
    children: [
      {
        element: <AdminLayout />,
        children: [
          {
            path: '/admin',
            element: (
              <Suspense fallback={null}>
                <DashboardPage />
              </Suspense>
            ),
          },
        ],
      },
    ],
  },
])

export default router
