// mock version of navigateToPage
export const navigateToPage = jest.fn((url: string) => {
    console.log('Navigating to:', url);
});
