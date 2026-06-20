export interface Team {
  id: string;
  name: string;
  description: string | null;
  teamLeadEmail: string | null;
  vulnerabilityCount: number;
}
