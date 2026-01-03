import { ImageInfo } from "./image-info.model";

export interface Report {
  folderId: string;
  images: ImageInfo[];
  address: string;
  temperature?: number;
  windSpeed?: number;
  windDirection: string;
  hoursOfSunshine ?: number;
  frontDoorDirection: string;
}

