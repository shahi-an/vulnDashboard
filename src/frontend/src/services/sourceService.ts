import { apiClient } from '@/lib/apiClient';
import type { VulnerabilitySource } from '@/types/source';

export const sourceService = {
  getAll: (activeOnly?: boolean) =>
    apiClient
      .get<VulnerabilitySource[]>('/api/sources', { params: activeOnly !== undefined ? { activeOnly } : {} })
      .then((r) => r.data),
};
