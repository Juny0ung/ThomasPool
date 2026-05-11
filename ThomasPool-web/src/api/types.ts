export const QuestionType = {
  ShortAnswer: 0,
  Essay: 1,
  MultipleChoice: 2,
  MultipleSelection: 3,
  TrueFalse: 4,
} as const

export type QuestionType = (typeof QuestionType)[keyof typeof QuestionType]

export interface QuestionFormDto {
  type: 'question'
  question: string
  questionType: QuestionType
}

export interface MultipleFormDto {
  type: 'multiple'
  question: string
  questionType: QuestionType
  options: string[]
}

export type AnyQuestion = QuestionFormDto | MultipleFormDto

export interface ProfileFormResponse {
  version: number
  questions: AnyQuestion[]
}

export interface SingleContentDto {
  type: 'string'
  value: string
}

export interface BoolContentDto {
  type: 'bool'
  value: boolean
}

export interface MultipleContentDto {
  type: 'multiple'
  values: string[]
}

export type ProfileContentDto = SingleContentDto | BoolContentDto | MultipleContentDto

export interface ProfileRequest {
  name: string
  gender: boolean
  phoneNumber: string
  birthYear: number
  region: string
  version: number
  info: ProfileContentDto[]
}

export interface ProfileResponse extends ProfileRequest {
  photoId: string
}
