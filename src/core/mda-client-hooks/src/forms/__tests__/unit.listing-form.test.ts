import { OnLoad } from '../listing-form';
import { XrmMockGenerator } from 'xrm-mock';

describe('OnLoad', () => {
    beforeEach(() => {
        XrmMockGenerator.initialise();
    });

    it('should execute the OnLoad function', async () => {
        // Mock the Xrm.Events.EventContext object
        const context = XrmMockGenerator.getEventContext();

        // Mock formContext.data.entity.getEntityName() to return 'contoso_listing'
        context.getFormContext().data.entity.getEntityName = jest.fn(() => 'contoso_listing');

        // Call the OnLoad function
        await OnLoad(context);

        // Expect  formContext.data.entity.getEntityName() to be called
        expect(context.getFormContext().data.entity.getEntityName).toHaveBeenCalled();
    });
});
