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

export interface UploadBatchDetail {
  id: string;
  originalFileName: string;
  rawFileBlobUri: string | null;
  sourceId: string;
  sourceName: string;
  status: string;
  totalRecords: number;
  processedCount: number;
  successCount: number;
  failureCount: number;
  errorSummary: string | null;
  uploadedBy: string;
  createdAt: string;
  updatedAt: string | null;
}

export const TERMINAL_STATUSES = new Set(['Completed', 'CompletedWithErrors', 'Failed', 'Cancelled']);

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

  getById: (id: string) =>
    apiClient.get<UploadBatchDetail>(`/api/upload-batches/${id}`).then((r) => r.data),
};
