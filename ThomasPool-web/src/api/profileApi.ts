import type { AnyQuestion, ProfileRequest, ProfileResponse, ProfileFormResponse } from './types'

const BASE_URL = import.meta.env.VITE_API_URL ?? ''

export async function getProfileForm(): Promise<ProfileFormResponse> {
  const res = await fetch(`${BASE_URL}/api/profile/profileform`)
  if (!res.ok) throw new Error('Failed to fetch profile form')
  return res.json()
}

export async function submitProfile(data: ProfileRequest, photo: File): Promise<void> {
  const formData = new FormData()
  formData.append('profile', JSON.stringify(data))
  formData.append('photo', photo)
  const res = await fetch(`${BASE_URL}/api/profile/profile`, {
    method: 'POST',
    body: formData,
  })
  if (!res.ok) throw new Error('Failed to submit profile')
}

export function getPhotoUrl(photoId: string): string {
  return `${BASE_URL}/api/profile/photo/${photoId}`
}

export async function updateProfileForm(token: string, questions: AnyQuestion[]): Promise<void> {
  const res = await fetch(`${BASE_URL}/api/profile/profileform`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
    body: JSON.stringify({ questions }),
  })
  if (!res.ok) throw new Error('폼 업데이트에 실패했습니다.')
}

export interface ProfileFilter {
  name?: string[]
  gender?: boolean[]
  birthYear?: number[]
  region?: string[]
}

export async function getProfiles(
  token: string,
  skip: number,
  limit: number,
  filter: ProfileFilter = {},
): Promise<ProfileResponse[]> {
  const params = new URLSearchParams({ skip: String(skip), limit: String(limit) })
  filter.name?.forEach((v) => params.append('name', v))
  filter.gender?.forEach((v) => params.append('gender', String(v)))
  filter.birthYear?.forEach((v) => params.append('birthYear', String(v)))
  filter.region?.forEach((v) => params.append('region', v))
  const res = await fetch(`${BASE_URL}/api/profile/profiles?${params}`, {
    headers: { Authorization: `Bearer ${token}` },
  })
  if (!res.ok) throw new Error('프로필 목록을 불러오는데 실패했습니다.')
  return res.json()
}
