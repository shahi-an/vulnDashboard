export type AssetType =
  | 'Unknown'
  | 'WebApplication'
  | 'Api'
  | 'MobileApplication'
  | 'CloudResource'
  | 'NetworkDevice'
  | 'Server'
  | 'Workstation'
  | 'Container';

export const ASSET_TYPE_LABELS: Record<AssetType, string> = {
  Unknown: 'Unknown',
  WebApplication: 'Web Application',
  Api: 'API',
  MobileApplication: 'Mobile Application',
  CloudResource: 'Cloud Resource',
  NetworkDevice: 'Network Device',
  Server: 'Server',
  Workstation: 'Workstation',
  Container: 'Container',
};

export interface Asset {
  id: string;
  name: string;
  type: AssetType;
  description?: string;
  owner?: string;
  environment?: string;
  createdAt: string;
}

export interface CreateAssetRequest {
  name: string;
  type: AssetType;
  description?: string;
  owner?: string;
  environment?: string;
}
