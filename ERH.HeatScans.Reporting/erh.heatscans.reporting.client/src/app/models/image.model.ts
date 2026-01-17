import { ImageScale } from "./image-scale.model";
import { ImageSpot } from "./image-spot.model";
import { ImageSize } from "./image-size.model";

export interface Image {
  data: string; // Base64 encoded string 
  mimeType: string;
  spots: ImageSpot[];
  dateTaken: string;
  scale?: ImageScale;
  daylightPhotoData: string; // Base64 encoded string
  size: ImageSize;
}
