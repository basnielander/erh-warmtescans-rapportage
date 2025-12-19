export interface GoogleDriveItem {
  id: string;
  name: string;
  mimeType: string;
  isFolder: boolean;
  modifiedTime?: string;
  size?: number;
  children: GoogleDriveItem[];
}
