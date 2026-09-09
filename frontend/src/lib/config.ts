const API_URL = process.env.NEXT_PUBLIC_API_URL?.replace(/\/+$/, "");

if (!API_URL) {
  const message = "Missing NEXT_PUBLIC_API_URL. Set it in frontend/.env.local during development or in your Amplify build environment before deployment.";
  console.error(message);
  throw new Error(message);
}

export { API_URL };
