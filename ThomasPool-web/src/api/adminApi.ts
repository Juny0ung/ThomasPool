const BASE_URL = import.meta.env.VITE_API_URL ?? ''

export interface AdminDto {
  id: string
  name: string
  role: number
}

export async function login(adminId: string, password: string): Promise<string> {
  const res = await fetch(`${BASE_URL}/api/admin/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ adminId, password }),
  })
  if (!res.ok) throw new Error('로그인에 실패했습니다.')
  const data = await res.json()
  return data.token as string
}

export async function register(name: string, adminId: string, password: string): Promise<void> {
  const res = await fetch(`${BASE_URL}/api/admin/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name, adminId, password }),
  })
  if (!res.ok) throw new Error('가입 신청에 실패했습니다.')
}

export async function getPendingList(token: string, skip: number, limit: number): Promise<AdminDto[]> {
  const res = await fetch(`${BASE_URL}/api/admin/pendinglist?skip=${skip}&limit=${limit}`, {
    headers: { Authorization: `Bearer ${token}` },
  })
  if (!res.ok) throw new Error('목록을 불러오는데 실패했습니다.')
  return res.json()
}

export async function approveAdmins(token: string, ids: string[]): Promise<void> {
  const res = await fetch(`${BASE_URL}/api/admin/approve`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
    body: JSON.stringify(ids),
  })
  if (!res.ok) throw new Error('승인에 실패했습니다.')
}
