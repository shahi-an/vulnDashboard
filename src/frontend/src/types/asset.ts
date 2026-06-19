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

export interface Asset {
  id: string;
  name: string;
  type: AssetType;
  description?: string;
  owner?: string;
  environment?: string;
}
