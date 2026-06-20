export interface VulnerabilitySource {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
  totalVulnerabilities: number;
  totalBatches: number;
}
