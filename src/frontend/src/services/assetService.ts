import { apiClient } from '@/lib/apiClient';
import type { Asset, CreateAssetRequest } from '@/types/asset';

export const assetService = {
  getAll: (params?: { search?: string; type?: string }) =>
    apiClient.get<Asset[]>('/api/assets', { params }).then((r) => r.data),

  getById: (id: string) =>
    apiClient.get<Asset>(`/api/assets/${id}`).then((r) => r.data),

  create: (payload: CreateAssetRequest) =>
    apiClient.post<string>('/api/assets', payload).then((r) => r.data),

  update: (id: string, payload: CreateAssetRequest) =>
    apiClient.put(`/api/assets/${id}`, payload),

  delete: (id: string) =>
    apiClient.delete(`/api/assets/${id}`),
};
