// Mock implementation of listingsApi.tsx
// eslint-disable-next-line @typescript-eslint/no-unused-vars
export const reserveListing = jest.fn(async (listingID: string, from: Date, to: Date, guests: number) => {
    return { sessionUrl: 'https://checkout' };
});
