export interface Report {
  folderId: string;
  images: ImageInfo[];
}

export interface ImageInfo {
  id: string;
  index: number;
  name: string;
  mimeType: string;
  size?: number;
  modifiedTime?: string;
  excludeFromReport?: boolean;
  outdoor?: boolean;
  temperatureMin?: number;
  temperatureMax?: number;
}
