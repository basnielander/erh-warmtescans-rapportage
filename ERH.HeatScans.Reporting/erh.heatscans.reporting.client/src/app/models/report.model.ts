export interface Report {
  folderId: string;
  images: ImageInfo[];
}

export interface ImageInfo {
  id: string;
  name: string;
  mimeType: string;
  size?: number;
  modifiedTime?: string;
}
