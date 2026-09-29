export type UserProfile = {
  userId: string;
  email: string;
  displayName: string;
  roles: string[];
};

export type LoginResponse = {
  accessToken: string;
  expiresAtUtc: string;
  user: UserProfile;
};

export type DocumentSummary = {
  id: string;
  displayName: string;
  originalFileName: string;
  status: number;
  sizeBytes: number;
  createdAtUtc: string;
};

export type ProblemDetailsBody = {
  title?: string;
  detail?: string;
};
