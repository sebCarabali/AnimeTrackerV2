/** Matches `HealthStatus` in contracts/public/bff.v1.yaml. */
export interface HealthStatus {
  status: string;
  timestamp: string;
  dependencies: Record<string, string>;
}

/** Matches `Problem` (RFC 9457) in contracts/public/bff.v1.yaml. */
export interface Problem {
  type: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
}
