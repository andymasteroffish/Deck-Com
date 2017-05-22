#run using “python3” not “python”
#You need to use the version you installed (python3) not the system version (python)

from __future__ import print_function
import httplib2
import os

from apiclient import discovery
from oauth2client import client
from oauth2client import tools
from oauth2client.file import Storage

import sys

try:
    import argparse
    flags = argparse.ArgumentParser(parents=[tools.argparser]).parse_args()
except ImportError:
    flags = None

# If modifying these scopes, delete your previously saved credentials
# at ~/.credentials/sheets.googleapis.com-python-quickstart.json
SCOPES = 'https://www.googleapis.com/auth/spreadsheets.readonly'
CLIENT_SECRET_FILE = 'client_secret.json'
APPLICATION_NAME = 'Google Sheets API Python Quickstart'

#equipment needs to store some info so we can make cards
equipmentNames = []
equipmentIDs = []
equipmentLevels = []
equipmentDescs = []

#google's code for grabbing the credentials
def get_credentials():
    home_dir = os.path.expanduser('~')
    credential_dir = os.path.join(home_dir, '.credentials')
    if not os.path.exists(credential_dir):
        os.makedirs(credential_dir)
    credential_path = os.path.join(credential_dir,
                                   'sheets.googleapis.com-python-quickstart.json')

    store = Storage(credential_path)
    credentials = store.get()
    if not credentials or credentials.invalid:
        flow = client.flow_from_clientsecrets(CLIENT_SECRET_FILE, SCOPES)
        flow.user_agent = APPLICATION_NAME
        if flags:
            credentials = tools.run_flow(flow, store, flags)
        else: # Needed only for compatibility with Python 2.6
            credentials = tools.run(flow, store)
        print('Storing credentials to ' + credential_path)
    return credentials

def getCharms():
    credentials = get_credentials()
    http = credentials.authorize(httplib2.Http())
    discoveryUrl = ('https://sheets.googleapis.com/$discovery/rest?'
                    'version=v4')
    service = discovery.build('sheets', 'v4', http=http,
                              discoveryServiceUrl=discoveryUrl)

    #id part of my spreadhseet url
    spreadsheetId = '1X2tsHz0IEwAQ4X9_Z7RRYn7qM3_m8j6yDkQu3e-c7WE'
    rangeName = 'charms!A2:P'
    result = service.spreadsheets().values().get(
        spreadsheetId=spreadsheetId, range=rangeName).execute()
    values = result.get('values', [])

    if not values:
        print('No card data found.')
    else:
        #we found stuff!
        pathname = os.path.dirname(os.path.realpath(__file__))

        file = open(pathname+'/charms.xml', 'w')
        file.write('<charms>\n')
        for row in values:
            #get all of the info
            displayName = row[0]
            idName = row[1]
            typeName = row[2]
            scriptName = row[3]
            level = row[4]
            if level == 'n/a':
                level = -1
            
            description = row[5]

            #remaining elements are additional values that the card might need
            otherVals = row[6:len(row)]
            
            file.write('<charm idName="'+idName+'">\n')
            file.write('<name>'+displayName+'</name>\n')
            file.write('<type>'+typeName+'</type>\n')
            file.write('<level>'+str(level)+'</level>\n')
            file.write('<script>'+scriptName+'</script>\n')
            if description != ".":
                file.write('<desc>'+description+'</desc>\n')

            for val in otherVals:
                #print(val)
                thisVal = val.split(':')
                file.write('<'+thisVal[0]+'>'+thisVal[1]+'</'+thisVal[0]+'>\n')

            file.write('</charm>\n')

            #We need to store some info for equpment so we can make cards for 'em
            if typeName == "Equipment":
                print("store "+idName)
                equipmentNames.append(displayName)
                equipmentIDs.append(idName)
                equipmentLevels.append(level)
                equipmentDescs.append(description)
            

        file.write('</charms>')
        file.close()



def getCards():
    credentials = get_credentials()
    http = credentials.authorize(httplib2.Http())
    discoveryUrl = ('https://sheets.googleapis.com/$discovery/rest?'
                    'version=v4')
    service = discovery.build('sheets', 'v4', http=http,
                              discoveryServiceUrl=discoveryUrl)

    #id part of my spreadhseet url
    spreadsheetId = '1X2tsHz0IEwAQ4X9_Z7RRYn7qM3_m8j6yDkQu3e-c7WE'
    rangeName = 'cards!A2:P'
    result = service.spreadsheets().values().get(
        spreadsheetId=spreadsheetId, range=rangeName).execute()
    values = result.get('values', [])

    if not values:
        print('No card data found.')
    else:
        #we found stuff!
        pathname = os.path.dirname(os.path.realpath(__file__))

        file = open(pathname+'/cards.xml', 'w')
        file.write('<cards>\n')
        for row in values:
            #get all of the info
            displayName = row[0]
            idName = row[1]
            level = row[2]
            if level == 'n/a':
                level = -1
            scriptName = row[3]
            description = row[4]

            #remaining elements are additional values that the card might need
            otherVals = row[5:len(row)]
            

            file.write('<card idName="'+idName+'">\n')

            file.write('<name>'+displayName+'</name>\n')
            file.write('<level>'+str(level)+'</level>\n')
            file.write('<script>'+scriptName+'</script>\n')
            if description != ".":
                file.write('<desc>'+description+'</desc>\n')

            for val in otherVals:
                thisVal = val.split(':')
                file.write('<'+thisVal[0]+'>'+thisVal[1]+'</'+thisVal[0]+'>\n')

            file.write('</card>\n')

        #now add equipment
        for i in range(0, len(equipmentNames)):
            print("add "+equipmentNames[i]) 
            file.write('<card idName="equipment_'+equipmentIDs[i]+'">\n')
            file.write('<name>'+equipmentNames[i]+'</name>\n')
            file.write('<level>'+str(equipmentLevels[i])+'</level>\n')
            file.write('<script>Card_Equipment</script>\n')
            file.write('<desc>'+equipmentDescs[i]+'</desc>\n')
            file.write('<equipment_id>'+equipmentIDs[i]+'</equipment_id>\n')
            file.write('</card>\n')

        file.write('</cards>')
        file.close()


if __name__ == '__main__':
    getCharms()
    getCards()
