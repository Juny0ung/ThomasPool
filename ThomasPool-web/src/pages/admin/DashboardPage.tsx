import { useState } from 'react'
import ProfileForm from '../../components/ProfileForm'
import ApprovePanel from './panels/ApprovePanel'
import ProfileFormPanel from './panels/ProfileFormPanel'
import ProfilesPanel from './panels/ProfilesPanel'

type ActivePanel = 'approve' | 'form' | 'profiles' | null

const PANELS: { key: NonNullable<ActivePanel>; label: string; icon: string }[] = [
  { key: 'approve', label: 'Admin 승인', icon: '✓' },
  { key: 'form', label: '설문 폼 관리', icon: '✎' },
  { key: 'profiles', label: '프로필 열람', icon: '☰' },
]

export default function DashboardPage() {
  const [active, setActive] = useState<ActivePanel>(null)

  function toggle(panel: NonNullable<ActivePanel>) {
    setActive((prev) => (prev === panel ? null : panel))
  }

  return (
    <div>
      <div className="flex gap-3 border-b border-gray-100 bg-white px-6 py-4">
        {PANELS.map(({ key, label, icon }) => (
          <button
            key={key}
            onClick={() => toggle(key)}
            className={`flex items-center gap-2 rounded-xl px-4 py-2 text-sm font-medium transition ${
              active === key
                ? 'bg-indigo-600 text-white shadow-sm'
                : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
            }`}
          >
            <span>{icon}</span>
            {label}
          </button>
        ))}
      </div>

      {active ? (
        <div className="px-6 py-6">
          {active === 'approve' && <ApprovePanel />}
          {active === 'form' && <ProfileFormPanel />}
          {active === 'profiles' && <ProfilesPanel />}
        </div>
      ) : (
        <ProfileForm />
      )}
    </div>
  )
}
