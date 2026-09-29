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

export type DocumentIngestionMetadata = {
  attemptCount: number;
  failureReason?: string | null;
  startedAtUtc?: string | null;
  completedAtUtc?: string | null;
};

export type DocumentSummary = {
  id: string;
  displayName: string;
  originalFileName: string;
  status: number;
  sizeBytes: number;
  createdAtUtc: string;
  ingestion: DocumentIngestionMetadata;
};

export type ProblemDetailsBody = {
  title?: string;
  detail?: string;
};
