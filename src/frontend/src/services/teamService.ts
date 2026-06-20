import { apiClient } from '@/lib/apiClient';
import type { Team } from '@/types/team';

export const teamService = {
  getAll: () => apiClient.get<Team[]>('/api/teams').then((r) => r.data),
};
