import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { VulnerabilityTable } from '@/components/vulnerability/VulnerabilityTable';
import { VulnerabilityFilters } from '@/components/vulnerability/VulnerabilityFilters';
import { CreateVulnerabilityModal } from '@/components/vulnerability/CreateVulnerabilityModal';
import { Pagination } from '@/components/ui/Pagination';
import { Spinner } from '@/components/ui/Spinner';
import { vulnerabilityService } from '@/services/vulnerabilityService';
import type { GetVulnerabilitiesParams } from '@/types/vulnerability';

const DEFAULT_FILTERS: GetVulnerabilitiesParams = { pageNumber: 1, pageSize: 25 };

export function VulnerabilitiesPage() {
  const [filters, setFilters] = useState<GetVulnerabilitiesParams>(DEFAULT_FILTERS);
  const [showCreate, setShowCreate] = useState(false);

  const { data, isLoading, isError } = useQuery({
    queryKey: ['vulnerabilities', filters],
    queryFn: () => vulnerabilityService.getAll(filters),
    placeholderData: (prev) => prev,
  });

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Vulnerabilities</h1>
          <p className="mt-0.5 text-sm text-gray-500">
            {data ? `${data.totalCount} total` : 'Loading…'}
          </p>
        </div>
        <button
          onClick={() => setShowCreate(true)}
          className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700"
        >
          + Add vulnerability
        </button>
      </div>

      <VulnerabilityFilters filters={filters} onChange={setFilters} />

      <div className="rounded-xl border border-gray-200 bg-white shadow-sm overflow-hidden">
        {isLoading && (
          <div className="flex justify-center py-16">
            <Spinner className="h-8 w-8" />
          </div>
        )}

        {isError && (
          <div className="py-16 text-center text-sm text-red-500">
            Failed to load vulnerabilities.
          </div>
        )}

        {data && <VulnerabilityTable items={data.items} />}

        {data && data.totalPages > 1 && (
          <Pagination
            pageNumber={data.pageNumber}
            totalPages={data.totalPages}
            totalCount={data.totalCount}
            pageSize={data.pageSize}
            onPageChange={(page) => setFilters((f) => ({ ...f, pageNumber: page }))}
          />
        )}
      </div>

      <CreateVulnerabilityModal open={showCreate} onClose={() => setShowCreate(false)} />
    </div>
  );
}
