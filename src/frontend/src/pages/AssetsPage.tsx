import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { format } from 'date-fns';
import { Modal } from '@/components/ui/Modal';
import { Spinner } from '@/components/ui/Spinner';
import { assetService } from '@/services/assetService';
import { ASSET_TYPE_LABELS, type Asset, type AssetType } from '@/types/asset';

const ASSET_TYPE_OPTIONS = Object.keys(ASSET_TYPE_LABELS) as AssetType[];

const assetSchema = z.object({
  name: z.string().min(1, 'Required').max(200),
  type: z.string().min(1, 'Required'),
  description: z.string().optional(),
  owner: z.string().optional(),
  environment: z.string().optional(),
});

type AssetFormValues = z.infer<typeof assetSchema>;

export function AssetsPage() {
  const [search, setSearch] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [editingAsset, setEditingAsset] = useState<Asset | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const queryClient = useQueryClient();

  const { data: assets = [], isLoading } = useQuery({
    queryKey: ['assets', { search, type: typeFilter }],
    queryFn: () =>
      assetService.getAll({
        search: search || undefined,
        type: typeFilter || undefined,
      }),
  });

  const createForm = useForm<AssetFormValues>({ resolver: zodResolver(assetSchema) });
  const editForm = useForm<AssetFormValues>({ resolver: zodResolver(assetSchema) });

  const createMutation = useMutation({
    mutationFn: (values: AssetFormValues) =>
      assetService.create({
        name: values.name,
        type: values.type as AssetType,
        description: values.description || undefined,
        owner: values.owner || undefined,
        environment: values.environment || undefined,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assets'] });
      createForm.reset();
      setShowCreate(false);
    },
  });

  const updateMutation = useMutation({
    mutationFn: (values: AssetFormValues) =>
      assetService.update(editingAsset!.id, {
        name: values.name,
        type: values.type as AssetType,
        description: values.description || undefined,
        owner: values.owner || undefined,
        environment: values.environment || undefined,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assets'] });
      setEditingAsset(null);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => assetService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assets'] });
      setDeletingId(null);
    },
  });

  const openEdit = (asset: Asset) => {
    setEditingAsset(asset);
    editForm.reset({
      name: asset.name,
      type: asset.type,
      description: asset.description ?? '',
      owner: asset.owner ?? '',
      environment: asset.environment ?? '',
    });
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Assets</h1>
          <p className="mt-1 text-sm text-gray-500">
            Registered servers and infrastructure assets.
          </p>
        </div>
        <button
          onClick={() => setShowCreate(true)}
          className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700"
        >
          <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          Add asset
        </button>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search by name or owner…"
          className="w-64 rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
        />
        <select
          value={typeFilter}
          onChange={(e) => setTypeFilter(e.target.value)}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
        >
          <option value="">All types</option>
          {ASSET_TYPE_OPTIONS.map((t) => (
            <option key={t} value={t}>{ASSET_TYPE_LABELS[t]}</option>
          ))}
        </select>
        {(search || typeFilter) && (
          <button
            onClick={() => { setSearch(''); setTypeFilter(''); }}
            className="text-sm text-gray-500 underline hover:text-gray-800"
          >
            Clear
          </button>
        )}
      </div>

      {/* Table */}
      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm">
        {isLoading ? (
          <div className="flex justify-center py-16"><Spinner className="h-8 w-8" /></div>
        ) : assets.length === 0 ? (
          <div className="py-16 text-center text-sm text-gray-400">
            {search || typeFilter
              ? 'No assets match the current filters.'
              : 'No assets registered yet. Click "Add asset" to get started.'}
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  {['Name', 'Type', 'Owner', 'Environment', 'Registered', ''].map((h) => (
                    <th
                      key={h}
                      className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500"
                    >
                      {h}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 bg-white">
                {assets.map((a) => (
                  <tr key={a.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3">
                      <p className="text-sm font-medium text-gray-900">{a.name}</p>
                      {a.description && (
                        <p className="max-w-[240px] truncate text-xs text-gray-400">
                          {a.description}
                        </p>
                      )}
                    </td>
                    <td className="whitespace-nowrap px-4 py-3 text-sm text-gray-600">
                      {ASSET_TYPE_LABELS[a.type]}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {a.owner ?? <span className="text-gray-300">—</span>}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {a.environment ?? <span className="text-gray-300">—</span>}
                    </td>
                    <td className="whitespace-nowrap px-4 py-3 text-xs text-gray-400">
                      {format(new Date(a.createdAt), 'dd MMM yyyy')}
                    </td>
                    <td className="whitespace-nowrap px-4 py-3 text-right">
                      <button
                        onClick={() => openEdit(a)}
                        className="mr-3 text-sm text-blue-600 hover:underline"
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => setDeletingId(a.id)}
                        className="text-sm text-red-500 hover:underline"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Create modal */}
      <AssetFormModal
        open={showCreate}
        title="Add asset"
        form={createForm}
        isPending={createMutation.isPending}
        isError={createMutation.isError}
        onClose={() => { setShowCreate(false); createForm.reset(); }}
        onSubmit={createMutation.mutate}
        submitLabel="Create asset"
      />

      {/* Edit modal */}
      <AssetFormModal
        open={!!editingAsset}
        title="Edit asset"
        form={editForm}
        isPending={updateMutation.isPending}
        isError={updateMutation.isError}
        onClose={() => setEditingAsset(null)}
        onSubmit={updateMutation.mutate}
        submitLabel="Save changes"
      />

      {/* Delete confirm modal */}
      <Modal
        open={!!deletingId}
        onClose={() => setDeletingId(null)}
        title="Delete asset"
      >
        <div className="space-y-4">
          <p className="text-sm text-gray-600">
            Are you sure you want to delete this asset? This action cannot be undone.
          </p>
          {deleteMutation.isError && (
            <p className="text-sm text-red-600">Failed to delete asset. Please try again.</p>
          )}
          <div className="flex justify-end gap-3">
            <button
              onClick={() => setDeletingId(null)}
              className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={() => deletingId && deleteMutation.mutate(deletingId)}
              disabled={deleteMutation.isPending}
              className="inline-flex items-center gap-2 rounded-lg bg-red-600 px-4 py-2 text-sm font-semibold text-white hover:bg-red-700 disabled:opacity-50"
            >
              {deleteMutation.isPending ? <Spinner className="h-4 w-4" /> : 'Delete'}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}

function AssetFormModal({
  open,
  title,
  form,
  isPending,
  isError,
  onClose,
  onSubmit,
  submitLabel,
}: {
  open: boolean;
  title: string;
  form: ReturnType<typeof useForm<AssetFormValues>>;
  isPending: boolean;
  isError: boolean;
  onClose: () => void;
  onSubmit: (values: AssetFormValues) => void;
  submitLabel: string;
}) {
  const { register, handleSubmit, formState: { errors } } = form;

  return (
    <Modal open={open} onClose={onClose} title={title}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <Field label="Name" error={errors.name?.message}>
          <input {...register('name')} placeholder="db-prod-01.internal" className={inputCls} />
        </Field>

        <Field label="Type" error={errors.type?.message}>
          <select {...register('type')} className={inputCls}>
            <option value="">Select type…</option>
            {ASSET_TYPE_OPTIONS.map((t) => (
              <option key={t} value={t}>{ASSET_TYPE_LABELS[t]}</option>
            ))}
          </select>
        </Field>

        <Field label="Description (optional)" error={errors.description?.message}>
          <textarea {...register('description')} rows={2} placeholder="Short description…" className={inputCls} />
        </Field>

        <div className="grid grid-cols-2 gap-3">
          <Field label="Owner (optional)" error={errors.owner?.message}>
            <input {...register('owner')} placeholder="team@company.com" className={inputCls} />
          </Field>
          <Field label="Environment (optional)" error={errors.environment?.message}>
            <input {...register('environment')} placeholder="Production" className={inputCls} />
          </Field>
        </div>

        {isError && (
          <p className="text-sm text-red-600">Failed to save asset. Please try again.</p>
        )}

        <div className="flex justify-end gap-3 pt-1">
          <button type="button" onClick={onClose} className={secondaryBtn}>
            Cancel
          </button>
          <button type="submit" disabled={isPending} className={primaryBtn}>
            {isPending ? <Spinner className="h-4 w-4" /> : submitLabel}
          </button>
        </div>
      </form>
    </Modal>
  );
}

function Field({
  label,
  error,
  children,
}: {
  label: string;
  error?: string;
  children: React.ReactNode;
}) {
  return (
    <div>
      <label className="mb-1 block text-sm font-medium text-gray-700">{label}</label>
      {children}
      {error && <p className="mt-1 text-xs text-red-600">{error}</p>}
    </div>
  );
}

const inputCls =
  'w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500';
const primaryBtn =
  'inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700 disabled:opacity-50';
const secondaryBtn =
  'rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50';
