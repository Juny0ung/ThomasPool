export class UnauthorizedError extends Error {
  constructor() {
    super('인증이 만료되었습니다. 다시 로그인해주세요.')
  }
}

export function assertOk(res: Response): void {
  if (res.status === 401) throw new UnauthorizedError()
  if (!res.ok) throw new Error(`Request failed: ${res.status}`)
}
