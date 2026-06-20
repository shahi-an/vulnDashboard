import { apiClient } from '@/lib/apiClient';

export interface UserSearchResult {
  id: string;
  displayName?: string;
  email?: string;
  jobTitle?: string;
}

export const userService = {
  search: (q: string) =>
    apiClient
      .get<UserSearchResult[]>('/api/users/search', { params: { q } })
      .then((r) => r.data),
};
