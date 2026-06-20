import { apiClient } from '@/lib/apiClient';

export interface UploadBatch {
  id: string;
  sourceId: string;
  sourceName: string;
  originalFileName: string;
  status: string;
  createdAt: string;
  createdBy: string;
}

export const uploadBatchService = {
  create: (sourceId: string, file: File) => {
    const form = new FormData();
    form.append('file', file);
    return apiClient
      .post<string>(`/api/upload-batches?sourceId=${encodeURIComponent(sourceId)}`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data);
  },

  getAll: (params?: { pageNumber?: number; pageSize?: number; sourceId?: string }) =>
    apiClient
      .get<{ items: UploadBatch[]; totalCount: number }>('/api/upload-batches', { params })
      .then((r) => r.data),
};
