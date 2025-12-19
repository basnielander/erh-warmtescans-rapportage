export interface GoogleUser {
  email: string;
  name: string;
  picture: string;
  sub: string;
}

export interface GoogleAuthResponse {
  credential: string;
  select_by: string;
}

export interface DecodedToken {
  iss: string;
  azp: string;
  aud: string;
  sub: string;
  email: string;
  email_verified: boolean;
  name: string;
  picture: string;
  given_name: string;
  family_name: string;
  iat: number;
  exp: number;
}
