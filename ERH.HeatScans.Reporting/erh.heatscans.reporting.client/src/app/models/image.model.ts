
export interface ImageInfo {
  id: string;
  index: number;
  name: string;
  comments?: string;
  mimeType: string;
  size?: number;
  modifiedTime?: string;
  excludeFromReport?: boolean;
  outdoor?: boolean;
  temperatureMin?: number;
  temperatureMax?: number;
}
