import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Spinner } from '@/components/ui/Spinner';
import { vulnerabilityService } from '@/services/vulnerabilityService';
import { teamService } from '@/services/teamService';
import { sourceService } from '@/services/sourceService';

interface StatCardProps {
  label: string;
  value: number | undefined;
  loading: boolean;
  accent?: 'red' | 'orange' | 'blue' | 'green';
}

const ACCENT_CLASSES = {
  red: 'border-l-red-500 bg-red-50',
  orange: 'border-l-orange-500 bg-orange-50',
  blue: 'border-l-blue-500 bg-blue-50',
  green: 'border-l-green-500 bg-green-50',
};

function StatCard({ label, value, loading, accent = 'blue' }: StatCardProps) {
  return (
    <div
      className={`flex flex-col gap-2 rounded-xl border border-gray-200 border-l-4 p-5 shadow-sm ${ACCENT_CLASSES[accent]}`}
    >
      <p className="text-sm font-medium text-gray-500">{label}</p>
      {loading ? (
        <Spinner className="h-6 w-6" />
      ) : (
        <p className="text-3xl font-bold text-gray-900">{value ?? '—'}</p>
      )}
    </div>
  );
}

export function DashboardPage() {
  const navigate = useNavigate();
  const [searchInput, setSearchInput] = useState('');
  const [teamId, setTeamId] = useState<string>('');
  const [sourceId, setSourceId] = useState<string>('');
  const [createdAfter, setCreatedAfter] = useState<string>('');
  const [createdBefore, setCreatedBefore] = useState<string>('');

  const filters = {
    ...(teamId ? { teamId } : {}),
    ...(sourceId ? { sourceId } : {}),
    ...(createdAfter ? { createdAfter } : {}),
    ...(createdBefore ? { createdBefore } : {}),
  };

  const teamsQ = useQuery({ queryKey: ['teams'], queryFn: () => teamService.getAll() });
  const sourcesQ = useQuery({ queryKey: ['sources'], queryFn: () => sourceService.getAll() });

  const openQ = useQuery({
    queryKey: ['vulnerabilities', { status: 'Open', pageSize: 1, ...filters }],
    queryFn: () => vulnerabilityService.getAll({ status: 'Open', pageSize: 1, ...filters }),
  });

  const criticalQ = useQuery({
    queryKey: ['vulnerabilities', { severity: 'Critical', status: 'Open', pageSize: 1, ...filters }],
    queryFn: () => vulnerabilityService.getAll({ severity: 'Critical', status: 'Open', pageSize: 1, ...filters }),
  });

  const highQ = useQuery({
    queryKey: ['vulnerabilities', { severity: 'High', status: 'Open', pageSize: 1, ...filters }],
    queryFn: () => vulnerabilityService.getAll({ severity: 'High', status: 'Open', pageSize: 1, ...filters }),
  });

  const remediatedQ = useQuery({
    queryKey: ['vulnerabilities', { status: 'Remediated', pageSize: 1, ...filters }],
    queryFn: () => vulnerabilityService.getAll({ status: 'Remediated', pageSize: 1, ...filters }),
  });

  const totalQ = useQuery({
    queryKey: ['vulnerabilities', { pageSize: 1, ...filters }],
    queryFn: () => vulnerabilityService.getAll({ pageSize: 1, ...filters }),
  });

  const critHighCount =
    (criticalQ.data?.totalCount ?? 0) + (highQ.data?.totalCount ?? 0);

  const hasFilters = !!teamId || !!sourceId || !!createdAfter || !!createdBefore;

  return (
    <div className="space-y-8">
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
          <p className="mt-1 text-sm text-gray-500">Summary of vulnerability posture across all servers.</p>
        </div>
        <form
          onSubmit={(e) => {
            e.preventDefault();
            if (searchInput.trim()) {
              navigate('/vulnerabilities', { state: { search: searchInput.trim() } });
            }
          }}
          className="flex items-center gap-2"
        >
          <input
            type="search"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            placeholder="Search vulnerabilities…"
            className="w-56 rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
          <button
            type="submit"
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
          >
            Search
          </button>
        </form>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        <select
          value={teamId}
          onChange={(e) => setTeamId(e.target.value)}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          disabled={teamsQ.isLoading}
        >
          <option value="">All teams</option>
          {teamsQ.data?.map((t) => (
            <option key={t.id} value={t.id}>{t.name}</option>
          ))}
        </select>

        <select
          value={sourceId}
          onChange={(e) => setSourceId(e.target.value)}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          disabled={sourcesQ.isLoading}
        >
          <option value="">All sources</option>
          {sourcesQ.data?.map((s) => (
            <option key={s.id} value={s.id}>{s.name}</option>
          ))}
        </select>

        <div className="flex items-center gap-1.5">
          <label className="text-xs text-gray-500 font-medium">From</label>
          <input
            type="date"
            value={createdAfter}
            onChange={(e) => setCreatedAfter(e.target.value)}
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>

        <div className="flex items-center gap-1.5">
          <label className="text-xs text-gray-500 font-medium">To</label>
          <input
            type="date"
            value={createdBefore}
            onChange={(e) => setCreatedBefore(e.target.value)}
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>

        {hasFilters && (
          <button
            onClick={() => { setTeamId(''); setSourceId(''); setCreatedAfter(''); setCreatedBefore(''); }}
            className="text-sm text-gray-500 hover:text-gray-800 underline"
          >
            Clear filters
          </button>
        )}
      </div>

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard
          label="Total vulnerabilities"
          value={totalQ.data?.totalCount}
          loading={totalQ.isLoading}
          accent="blue"
        />
        <StatCard
          label="Open"
          value={openQ.data?.totalCount}
          loading={openQ.isLoading}
          accent="red"
        />
        <StatCard
          label="Critical / High open"
          value={criticalQ.isLoading || highQ.isLoading ? undefined : critHighCount}
          loading={criticalQ.isLoading || highQ.isLoading}
          accent="orange"
        />
        <StatCard
          label="Remediated"
          value={remediatedQ.data?.totalCount}
          loading={remediatedQ.isLoading}
          accent="green"
        />
      </div>

      <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
        <h2 className="mb-4 text-sm font-semibold text-gray-700">Open by severity</h2>
        <SeverityBar
          critical={criticalQ.data?.totalCount ?? 0}
          high={highQ.data?.totalCount ?? 0}
          open={openQ.data?.totalCount ?? 0}
          loading={criticalQ.isLoading || highQ.isLoading || openQ.isLoading}
        />
      </div>
    </div>
  );
}

function SeverityBar({
  critical,
  high,
  open,
  loading,
}: {
  critical: number;
  high: number;
  open: number;
  loading: boolean;
}) {
  if (loading) return <Spinner />;
  if (open === 0) return <p className="text-sm text-gray-400">No open vulnerabilities.</p>;

  const pct = (n: number) => Math.round((n / open) * 100);

  const rows: { label: string; value: number; color: string }[] = [
    { label: 'Critical', value: critical, color: 'bg-red-500' },
    { label: 'High', value: high, color: 'bg-orange-400' },
    { label: 'Other', value: Math.max(0, open - critical - high), color: 'bg-gray-300' },
  ];

  return (
    <div className="space-y-3">
      {rows.map(({ label, value, color }) => (
        <div key={label} className="flex items-center gap-3">
          <span className="w-16 text-right text-xs font-medium text-gray-500">{label}</span>
          <div className="flex-1 rounded-full bg-gray-100 h-3">
            <div
              className={`h-3 rounded-full ${color} transition-all`}
              style={{ width: `${pct(value)}%` }}
            />
          </div>
          <span className="w-8 text-xs text-gray-500">{value}</span>
        </div>
      ))}
    </div>
  );
}
