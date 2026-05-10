import { useState } from 'react'
import ProfileForm from '../../components/ProfileForm'
import ApprovePanel from './panels/ApprovePanel'
import ProfileFormPanel from './panels/ProfileFormPanel'
import ProfilesPanel from './panels/ProfilesPanel'

type ActivePanel = 'approve' | 'form' | 'profiles' | null

const PANEL_LABELS: Record<NonNullable<ActivePanel>, string> = {
  approve: 'Admin 승인',
  form: '설문 폼 관리',
  profiles: '프로필 열람',
}

export default function DashboardPage() {
  const [active, setActive] = useState<ActivePanel>(null)

  function toggle(panel: NonNullable<ActivePanel>) {
    setActive((prev) => (prev === panel ? null : panel))
  }

  return (
    <div>
      <div>
        <button onClick={() => toggle('approve')}>Admin 승인</button>
        <button onClick={() => toggle('form')}>설문 폼 관리</button>
        <button onClick={() => toggle('profiles')}>프로필 열람</button>
      </div>

      {active && (
        <div>
          <h2>{PANEL_LABELS[active]}</h2>
          {active === 'approve' && <ApprovePanel />}
          {active === 'form' && <ProfileFormPanel />}
          {active === 'profiles' && <ProfilesPanel />}
        </div>
      )}

      <ProfileForm />
    </div>
  )
}
