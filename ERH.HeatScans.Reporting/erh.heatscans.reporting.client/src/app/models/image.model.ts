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

export interface Image {
  data: string;  // Base64 encoded string from C# byte[]
  mimeType: string;
  spots: ImageSpot[];
  dateTaken: string;
}

export interface ImageSpot {
  id: string;
  name: string;
  temperature: string;
  point: ImageSpotPoint;
}

export interface ImageSpotPoint {
  x: number;
  y: number;
}
